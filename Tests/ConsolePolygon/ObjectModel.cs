using System;
using MathCore.Vectors;

namespace ConsolePolygon
{
    internal class ObjectModel
    {
        public static void Test()
        {
            var m = 10d;
            var obj = new ObjectModel(m, 0.1);
            var t = 0d;
            var dt = 0.001;
            var g = 9.8;
            Vector3D F0(double time) => new(time <= 0.2 ? 1000 - g * m : -m * g, 0, 0);
            const double mu = 1.002;
            var k1 = 6 * Math.PI * mu * obj.Radius;
            const double c = 0.4;
            const double rho = 1.225;
            var k2 = c / 2 * 2 * Math.PI * obj.Radius * rho;
            Console.WriteLine($"t:{t:f2}|x:{obj.Position.X:0.####}|v:{obj.Speed.X:0.####}|a:{obj.Acceleration.X:0.####}");
            while(obj.Position.X >= 0)
            {
                t += dt;

                var v = obj.Speed.Abs;

                var Ft = v.Product_Component(k1 + v * k2);
                var f0 = F0(t);
                //if (f0.Sign.X < 0)
                //    Console.ReadLine();
                var F = f0 - obj.Speed.Sign.Product_Component(Ft);
                obj.CheckPosition(t, F);

                Console.WriteLine($"t:{t:f3}|x:{obj.Position.X:f6}|v:{obj.Speed.X:f4}|a:{obj.Acceleration.X:f4} ||F:{F.X}");
                //Thread.Sleep(10);
            }
            Console.ReadLine();
        }

        private Vector3D _LastPosition;
        private Vector3D _Position;
        private Vector3D _LastSpeed;
        private Vector3D _Speed;
        private Vector3D _LastAcceleration;
        private Vector3D _Acceleration;
        private double _Last_dt;

        private readonly double _Mass;
        private readonly double _Radius;

        public double Mass => _Mass;
        public double Radius => _Radius;

        public Vector3D Position => _Position;
        public Vector3D LastMoving => _Position - _LastPosition;
        public Vector3D LastRealSpeed => LastMoving / _Last_dt;
        public Vector3D Speed => _Speed;
        public Vector3D LastChangeOfSpeed => _Speed - _LastSpeed;
        public Vector3D LastRealAcceleration => LastChangeOfSpeed / _Last_dt;
        public Vector3D Acceleration => _Acceleration;
        public Vector3D LastChangeOfAcceleration => _Acceleration - _LastAcceleration;
        public Vector3D LastRealAccelerationBoost => LastChangeOfAcceleration / _Last_dt;


        public ObjectModel
        (
            double Mass,
            double Radius,
            Vector3D Position = new(),
            Vector3D Speed = new(),
            Vector3D Axeleration = new()
        )
        {
            _Mass = Mass;
            _Radius = Radius;
            _LastPosition = _Position = Position;
            _LastSpeed = _Speed = Speed;
            _LastAcceleration = _Acceleration = Axeleration;
        }

        public void CheckPosition(double dt, Vector3D Force)
        {
            //const double c32 = 3d / 2;
            //const double c12 = 1d / 2;
            _LastAcceleration = Acceleration;
            _LastSpeed = _Speed;
            _LastPosition = _Position;

            _Acceleration = Force / _Mass;

            //_Speed += dt * (c32 * _Acceleration - c12 * _LastAcceleration);
            //_Position += dt * (c32 * _Speed - c12 * _LastSpeed);

            _Speed += dt * (_Acceleration + _LastAcceleration) / 2;
            _Position += dt * (_Speed + _LastSpeed) / 2;

            _Last_dt = dt;
        }
    }
}