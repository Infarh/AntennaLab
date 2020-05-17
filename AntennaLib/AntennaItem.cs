using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas
{
    /// <summary>Антенный элемент</summary>
    public class AntennaItem : Antenna
    {
        private const double __PiC = Consts.pi2 / Consts.SpeedOfLight;

        private string _Name;
        private SpaceAngle _Direction;
        private Vector3D _Location;
        private Antenna _Element;
        private Complex _K;
        private Func<SpaceAngle, SpaceAngle> _Rotator;

        public string Name { get => _Name; set => Set(ref _Name, value); }

        /// <summary>Антенный элемент</summary>
        public Antenna Element
        {
            get => _Element;
            set
            {
                if(Equals(_Element, value)) return;
                {
                    if (_Element is INotifyPropertyChanged property_changed_obj)
                        property_changed_obj.PropertyChanged -= OnElementPropertyChanged;
                }   
                _Element = value;
                {
                    if (value is INotifyPropertyChanged property_changed_obj)
                        property_changed_obj.PropertyChanged += OnElementPropertyChanged;
                }
                OnPropertyChanged();
            }
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(Element)); 

        /// <summary>Вектор расположения антенного элемента решётки относительно её фазового центра</summary>
        public Vector3D Location { get => _Location; set => Set(ref _Location, value); }

        public double LocationX { get => _Location.X; set => Location = new Vector3D(value, LocationY, LocationZ); }

        public double LocationY { get => _Location.Y; set => Location = new Vector3D(LocationX, value, LocationZ); }

        public double LocationZ { get => _Location.Z; set => Location = new Vector3D(LocationX, LocationY, value); }

        /// <summary>Пространственный угол поворота антенного элемента относительно фазового центра антенного элемента</summary>
        public SpaceAngle Direction
        {
            get => _Direction;
            set
            {
                if (_Direction.Equals(value)) return;
                _Direction = value;
                _Rotator = value.Theta.Equals(0d) && value.Phi.Equals(0d)
                    ? null
                    : value.GetRotatorPhiTheta();
                OnPropertyChanged();
            }
        }

        public double Theta { get => _Direction.InRad.Theta; set => Direction = new SpaceAngle(value, Phi); }

        public double ThetaDeg { get => Theta / Consts.ToRad; set => Theta = value * Consts.ToRad; }

        public double Phi { get => _Direction.InRad.Phi; set => Direction = new SpaceAngle(Theta, value); }

        public double PhiDeg { get => Phi / Consts.ToRad; set => Phi = value * Consts.ToRad; }

        /// <summary>Комплексный коэффициент передачи антенного эелмента</summary>
        public Complex K { get => _K; set => Set(ref _K, value); }

        public double AbsK { get => K.Abs; set => K = Complex.Exp(value, ArgK); }

        public double ArgK { get => K.Abs; set => K = Complex.Exp(AbsK, value); }

        public double ReK { get => K.Re; set => K = new Complex(value, ImK); }

        public double ImK { get => K.Im; set => K = new Complex(ReK, value); }

        public AntennaItem() : this(new UniformAntenna(), Vector3D.Empty, SpaceAngle.k, Complex.Real) { }

        /// <summary>Инициализация нового антеннойго элемента антенной решётки</summary>
        /// <param name="a">Антенный элемент</param>
        /// <param name="r">Вектор размещения</param>
        /// <param name="angle">Угол поворота</param>
        /// <param name="k">Комплексный коэффициент передачи</param>
        public AntennaItem(Antenna a, Vector3D r, SpaceAngle angle, Complex k)
        {
            PropertyDependence_Add(nameof(Location), nameof(LocationX), nameof(LocationY), nameof(LocationZ));
            PropertyDependence_Add(nameof(Direction), nameof(Theta), nameof(ThetaDeg), nameof(Phi), nameof(PhiDeg));
            PropertyDependence_Add(nameof(K), nameof(AbsK), nameof(ArgK), nameof(ReK), nameof(ImK));

            _Element = a;
            _Location = r;
            Direction = angle;
            K = k;
        }

        /// <summary>Диаграмма направленности элемента относительно фазового центра решётки</summary>
        /// <param name="a">Направление</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение диаграммы направленности в указанном направлении на указанной частоте</returns>
        public override Complex Pattern(SpaceAngle a, double f)
        {
            if (_Rotator != null) a = _Rotator(a);
            return _K
                * _Element.Pattern(a, f)
                * Complex.Exp(-_Location.GetProjectionTo(a) * __PiC * f);
        }

        #region Overrides of Antenna

        private static BinaryExpression M(Expression a, Expression b) => Expression.Multiply(a, b);
        private static BinaryExpression M(Expression a, Expression b, Expression c) => M(a, M(b, c));
        private static Expression Const(object value) => Expression.Constant(value);
        private static MethodCallExpression Call(Delegate d, Expression arg) => Expression.Call(d.Method, arg);
        private static MethodCallExpression Call<T>(T obj, MethodInfo method, params Expression[] arg) => Expression.Call(Const(obj), method, arg);
        private static MethodCallExpression Call<T>(T obj, Delegate d, params Expression[] arg) => Expression.Call(Const(obj), d.Method, arg);

        public override Expression GetPatternExpressionBody(Expression a, Expression f)
        {
            if (_Rotator != null) a = Expression.Invoke(_Rotator.ToExpression(), a);
            var kl = (-__PiC).ToExpression().Multiply(f);
            var projection_info = typeof(Vector3D).GetMethod(nameof(Vector3D.GetProjectionTo), new[] { typeof(SpaceAngle) }, null);
            var projection = Call(Location, projection_info, a);
            var kr = kl.Multiply(projection);
            var exp = ((Func<double, Complex>)Complex.Exp).GetCallExpression(kr);
            return _Element.GetPatternExpressionBody(a, f).Multiply(_K.ToExpression().Multiply(exp));
        }

        #endregion

        public override string ToString()
        {
            var result = new StringBuilder();
            var empty = true;
            var r = _Location;
            if (r != 0)
            {
                empty = false;
                result.AppendFormat("[loc:{0}", r);
            }

            var a = _Direction;
            var angle_empty = true;
            if (!a.IsZero)
            {
                if (!empty) result.Append(" - ");
                result.AppendFormat(empty ? "[angle:{0}]" : "angle:{0}]", a);
                angle_empty = empty = false;
            }

            if (!empty && angle_empty) result.Append("]");

            return _K == 0 
                ? empty 
                    ? $"{{{Element}}}" 
                    : $"{{{Element}}}{result}" 
                : $"{{{Element}}}{result} x {_K}";
        }
    }
}