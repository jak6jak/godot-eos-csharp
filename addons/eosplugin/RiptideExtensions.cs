using Godot;

namespace Riptide
{
    /// <summary>
    /// Extension methods for RiptideNetworking to support Godot's Vector3 type
    /// </summary>
    public static class RiptideExtensions
    {
        /// <summary>Adds a <see cref="Vector3"/> to the message.</summary>
        /// <param name="vector3">The <see cref="Vector3"/> to add.</param>
        /// <returns>The message that the <see cref="Vector3"/> was added to.</returns>
        public static Message AddVector3(this Message message, Vector3 vector3)
        {
            return message.AddFloat(vector3.X).AddFloat(vector3.Y).AddFloat(vector3.Z);
        }

        /// <summary>Retrieves a <see cref="Vector3"/> from the message.</summary>
        /// <returns>The <see cref="Vector3"/> that was retrieved.</returns>
        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        /// <summary>Adds a <see cref="Vector2"/> to the message.</summary>
        /// <param name="vector2">The <see cref="Vector2"/> to add.</param>
        /// <returns>The message that the <see cref="Vector2"/> was added to.</returns>
        public static Message AddVector2(this Message message, Vector2 vector2)
        {
            return message.AddFloat(vector2.X).AddFloat(vector2.Y);
        }

        /// <summary>Retrieves a <see cref="Vector2"/> from the message.</summary>
        /// <returns>The <see cref="Vector2"/> that was retrieved.</returns>
        public static Vector2 GetVector2(this Message message)
        {
            return new Vector2(message.GetFloat(), message.GetFloat());
        }

        /// <summary>Adds a <see cref="Quaternion"/> to the message.</summary>
        /// <param name="quaternion">The <see cref="Quaternion"/> to add.</param>
        /// <returns>The message that the <see cref="Quaternion"/> was added to.</returns>
        public static Message AddQuaternion(this Message message, Quaternion quaternion)
        {
            return message.AddFloat(quaternion.X).AddFloat(quaternion.Y).AddFloat(quaternion.Z).AddFloat(quaternion.W);
        }

        /// <summary>Retrieves a <see cref="Quaternion"/> from the message.</summary>
        /// <returns>The <see cref="Quaternion"/> that was retrieved.</returns>
        public static Quaternion GetQuaternion(this Message message)
        {
            return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        /// <summary>Adds a <see cref="Transform3D"/> to the message.</summary>
        /// <param name="transform">The <see cref="Transform3D"/> to add.</param>
        /// <returns>The message that the <see cref="Transform3D"/> was added to.</returns>
        public static Message AddTransform3D(this Message message, Transform3D transform)
        {
            // Add origin (position)
            message.AddVector3(transform.Origin);
            
            // Add basis as quaternion (rotation)
            message.AddQuaternion(transform.Basis.GetRotationQuaternion());
            
            // Add scale
            message.AddVector3(transform.Basis.Scale);
            
            return message;
        }

        /// <summary>Retrieves a <see cref="Transform3D"/> from the message.</summary>
        /// <returns>The <see cref="Transform3D"/> that was retrieved.</returns>
        public static Transform3D GetTransform3D(this Message message)
        {
            Vector3 origin = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            Vector3 scale = message.GetVector3();
            
            Transform3D transform = new Transform3D();
            transform.Origin = origin;
            transform.Basis = new Basis(rotation).Scaled(scale);
            
            return transform;
        }

        /// <summary>Adds a <see cref="Color"/> to the message.</summary>
        /// <param name="color">The <see cref="Color"/> to add.</param>
        /// <returns>The message that the <see cref="Color"/> was added to.</returns>
        public static Message AddColor(this Message message, Color color)
        {
            return message.AddFloat(color.R).AddFloat(color.G).AddFloat(color.B).AddFloat(color.A);
        }

        /// <summary>Retrieves a <see cref="Color"/> from the message.</summary>
        /// <returns>The <see cref="Color"/> that was retrieved.</returns>
        public static Color GetColor(this Message message)
        {
            return new Color(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }
    }
}