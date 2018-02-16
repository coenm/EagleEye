namespace EagleEye.Core
{
    public class Camera
    {
        public Camera(string brand, string model)
        {
            Brand = brand;
            Model = model;
        }

        public Camera()
        {
        }

        public string Brand { get; set; }

        public string Model { get; set; }
    }
}