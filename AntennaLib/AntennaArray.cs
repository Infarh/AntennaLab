using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MathCore;
using MathCore.Annotations;
using MathCore.Values;
using MathCore.Vectors;
// ReSharper disable EmptyArray

namespace Antennas
{
    /// <summary>Антенная решётка</summary>
    public class AntennaArray : Antenna, IList<AntennaItem>
    {
        /// <summary>Создать линейную антенную решётку</summary>
        /// <param name="antennas">Перечисление антенных элементов</param>
        /// <param name="d">Шаг между элементами</param>
        /// <returns><see cref="AntennaArray"/>></returns>
        public static AntennaArray CreateLinearArray([NotNull]IEnumerable<Antenna> antennas, double d) => new LinearAntennaArray(antennas, d);

        /// <summary>Создать линейную антенную решётку</summary>
        /// <param name="antennas">Массив антенных элементов</param>
        /// <param name="step">Шаг размещения</param>
        /// <param name="A">Амплитудно-фазовое распределение</param>
        /// <returns>Антенная решётка с линейно размещёнными в пространстве антенными элементами</returns>
        public static AntennaArray CreateLinearArray(Antenna[] antennas, double step, Func<Vector3D, Complex> A = null)
        {
            var n = antennas.Length;
            var l05 = step * (n - 1) / 2;
            A ??= _ => 1;
            var items = antennas.Select((a, i) => new { a, v = new Vector3D(i * step - l05), o = new SpaceAngle() })
                        .Select(a => new { a.a, a.v, a.o, A = A(a.v) })
                        .Select(a => new AntennaItem(a.a, a.v, a.o, a.A))
                        .ToArray();

            return new AntennaArray(items);
        }

        /// <summary>Создать плоскую антенную решётку</summary>
        /// <param name="antennas">Массив антенных элементов</param>
        /// <param name="dx">Шаг по оси OX</param>
        /// <param name="dy">ШАг по оси OY</param>
        /// <returns>Антенная решётка с размещением антенных элементов в плоскости XOY</returns>
        public static AntennaArray CreateFlatArray(Antenna[,] antennas, double dx, double dy)
        {
            var nx = antennas.GetLength(0);
            var ny = antennas.GetLength(1);
            var lx05 = dx * (nx - 1);
            var ly05 = dy * (ny - 1);
            var items = new AntennaItem[nx * ny];
            for(int i = 0, ii = 0; i < nx; i++)
                for(var j = 0; j < ny; j++)
                    items[ii++] = new AntennaItem(antennas[i, j], new Vector3D(i * dx - lx05, j * dy - ly05), new SpaceAngle(), 1);
            return new AntennaArray(items);
        }

        /// <summary>Список антенных элементов</summary>
        [NotNull]
        private AntennaItem[] _Items;

        /// <summary>Размер апертуры</summary>
        public Vector3D ApertureLength => _Items.Aggregate(new { X = new MinMaxValue(), Y = new MinMaxValue(), Z = new MinMaxValue() },
                (R, i) =>
                {
                    var (x, y, z) = i.Location;
                    R.X.AddValue(x);
                    R.Y.AddValue(y);
                    R.Z.AddValue(z);
                    return R;
                },
                R => new Vector3D(R.X.Interval.Length, R.Y.Interval.Length, R.Z.Interval.Length));

        /// <summary>Размер апертуры по оси OX</summary>
        public double L_x =>
            _Items.Aggregate(new MinMaxValue(), (I, i) =>
            {
                I.AddValue(i.Location.X);
                return I;
            }, I => I.Interval.Length);

        /// <summary>Размер апертуры по оси OY</summary>
        public double L_y => _Items.Aggregate(new MinMaxValue(), (I, i) =>
            {
                I.AddValue(i.Location.Y);
                return I;
            }, I => I.Interval.Length);

        /// <summary>Размер апертуры по оси OZ</summary>
        public double L_z =>
            _Items.Aggregate(new MinMaxValue(), (I, i) =>
            {
                I.AddValue(i.Location.Z);
                return I;
            }, I => I.Interval.Length);

        /// <summary>Инициализация новой антенной решётки</summary>
        /// <param name="items">Перечисление антенных элементов</param>
        public AntennaArray(IEnumerable<AntennaItem> items) => _Items = items.ToArray();

        /// <summary>Диаграмма направленности антенной решётки</summary>
        /// <param name="Direction">Пространственный угол</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение ДН</returns>
        public override Complex Pattern(SpaceAngle Direction, double f) => _Items.Length == 0 ? Complex.NaN : _Items.AsParallel().Sum(i => i.Pattern(Direction, f));

        public double GetBeamPatternWidthX_deg(double lamda) => 51 * lamda / L_x;
        public double GetBeamPatternWidthY_deg(double lamda) => 51 * lamda / L_y;
        public double GetBeamPatternWidthZ_deg(double lamda) => 51 * lamda / L_z;


        public void SaveToFile(string FileName)
        {
            var info = new FileInfo(FileName);
            using var writer = info.CreateText();
            var title = new[]
            {
                "#",
                "X",
                "Y",
                "Z",
                "Theta",
                "Phi",
                "A",
                "Phase",
                "Element"
            };
            writer.WriteLine(title.ToSeparatedStr("\t"));
            var elements = this as IEnumerable<AntennaItem>;
            elements.Select((e, i) => new { i, e.Location, Direction = e.Direction.InDeg, e.K, e.Element })
               .Select(e => new
                {
                    e.i,
                    e.Location.X,
                    e.Location.Y,
                    e.Location.Z,
                    e.Direction.Theta,
                    e.Direction.Phi,
                    A = e.K.Abs.In_dB(),
                    Phase = e.K.Arg.ToDeg(),
                    Element = e.Element.GetType().Name
                })
               .Select(e => new object[] { e.i, e.X, e.Y, e.Z, e.Theta, e.Phi, e.A, e.Phase, e.Element })
               .Select(e => e.ToSeparatedStr("\t"))
               .Foreach(writer.WriteLine);
        }

        #region IList<AntennaArray> implementation
        public IEnumerator<AntennaItem> GetEnumerator() => _Items.Cast<AntennaItem>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Добавляет элемент в интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        /// <param name="item">Объект, добавляемый в интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.ICollection`1"/> доступен только для чтения.</exception>
        public virtual void Add(AntennaItem item)
        {
            if(ReferenceEquals(item?.Element, this))
                throw new ArgumentException(@"Произведена попытка добавить саму антенную решётку к себе в список антенных элементов", nameof(item));
            var new_items = new AntennaItem[_Items.Length + 1];
            Array.Copy(_Items, new_items, _Items.Length);
            new_items[^1] = item;
            _Items = new_items;
        }

        /// <summary>Удаляет все элементы из интерфейса <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        public virtual void Clear() => _Items = Array.Empty<AntennaItem>();

        public virtual bool Contains(AntennaItem item) => item != null && _Items.Contains(item);

        public virtual void CopyTo(AntennaItem[] array, int index) => _Items.CopyTo(array, index);

        public virtual bool Remove(AntennaItem item)
        {
            var new_items = _Items.ToList();
            var result = new_items.Remove(item);
            if (result) _Items = new_items.ToArray();
            return result;
        }

        public virtual int Count => _Items.Length;

        bool ICollection<AntennaItem>.IsReadOnly => false;

        public virtual int IndexOf(AntennaItem item) => Array.IndexOf(_Items, item);

        /// <summary>Вставляет элемент в список <see cref="T:System.Collections.Generic.IList`1"/> по указанному индексу.</summary>
        /// <param name="index">Индекс (с нуля), по которому следует вставить параметр <paramref name="item"/>.</param><param name="item">Объект, вставляемый в <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        public void Insert(int index, AntennaItem item)
        {
            Debug.Assert(item != null, nameof(item) + " != null");
            if(ReferenceEquals(item.Element, this) || Array.Exists(_Items, i => ReferenceEquals(i, item)))
                throw new InvalidOperationException();
            var items = _Items.ToList();
            items.Insert(index, item);
            _Items = items.ToArray();
        }

        /// <summary>Удаляет элемент <see cref="T:System.Collections.Generic.IList`1"/> по указанному индексу.</summary>
        /// <param name="index">Индекс (с нуля) удаляемого элемента.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        public virtual void RemoveAt(int index)
        {
            var new_items = _Items.ToList();
            new_items.RemoveAt(index);
            _Items = new_items.ToArray();
        }

        /// <summary>Получает или задает элемент по указанному индексу.</summary>
        /// <returns>Элемент с указанным индексом.</returns>
        /// <param name="index">Индекс (с нуля) элемента, который необходимо получить или задать.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Свойство задано, и объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        [NotNull]
        public virtual AntennaItem this[int index]
        {
            get => _Items[index];
            set => _Items[index] = value;
        }
        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddRange([NotNull]IEnumerable<AntennaItem> collection)
        {
            collection = collection.ForeachLazy(i =>
            {
                if(ReferenceEquals(i.Element, this))
                    throw new ArgumentException(@"Произведена попытка добавить саму антенную решётку к себе в список антенных элементов", nameof(collection));
            });
            var to_add = collection.ToArray();
            var new_items = new AntennaItem[_Items.Length + to_add.Length];
            Array.Copy(_Items, new_items, _Items.Length);
            Array.Copy(to_add, 0, new_items, _Items.Length, new_items.Length);
            _Items = new_items;
        }

        #region Overrides of Antenna

        private static BinaryExpression Add([NotNull]Expression a, [NotNull]Expression b) => Expression.Add(a, b);

        public override Expression GetPatternExpressionBody(Expression a, Expression f) =>
            this
               .Select(i => i.GetPatternExpressionBody(a, f))
               .Aggregate((x, y) => x is null ? y : Add(x, y));

        #endregion
    }
}