//-----------------------------------------------------------------------------
// Camera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
//Some of the code in this class has been taken form the XNA Chase Camera sample
//and has been refractered by myself to work along with my code.



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Flight
{
    public class ChaseCamera
    {        
        // Position of player        
        public Vector3 chaseCamPosition
        {
            get { return chasePosition; }
            set { chasePosition = value; }
        }
        private Vector3 chasePosition;
        
        //Direction the camera is facing        
        public Vector3 ChaseDirection
        {
            get { return chaseDirection; }
            set { chaseDirection = value; }
        }
        private Vector3 chaseDirection;
       
        //get the cameras Up vector        
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        private Vector3 up = Vector3.Up;
       
        //get the desired camera offset position in the players world       
        public Vector3 DesiredPositionOffset
        {
            get { return desiredPositionOffset; }
            set { desiredPositionOffset = value; }
        }
        private Vector3 desiredPositionOffset = new Vector3(0, 2.0f, 2.0f);
        
        //get the desired camera position in world space        
        public Vector3 DesiredPosition
        {
            get { UpdateWorldPositions(); return desiredPosition; }
        }
        private Vector3 desiredPosition;
        
        //get the Look at offest in the players coordinate system        
        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }
        private Vector3 lookAtOffset = new Vector3(0, 2.8f, 0);
        
        // Look at point in world space        
        public Vector3 LookAt
        {
            get { UpdateWorldPositions(); return lookAt; }
        }
        private Vector3 lookAt;       
        
        //get camera stiffness he stiffer the spring, the closer it will stay to the player      
        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
        }
        private float stiffness = 1800.0f;

        //get damping of the spring
        public float Damping
        {
            get { return damping; }
            set { damping = value; }
        }
        private float damping = 600.0f;

        //Mass of the camera if the player mass is high then this must also be set high
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }
        private float mass = 50.0f;
        
        // Position of camera in world space        
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector3 position;

        
        // Velocity of camera        
        public Vector3 Velocity
        {
            get { return velocity; }
        }
        private Vector3 velocity;
        
        //get aspect ratio       
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }
        private float aspectRatio = 4.0f / 3.0f;

        
        //get field of view        
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; }
        }
        private float fieldOfView = MathHelper.ToRadians(45.0f);
        
        //get the near clipping plane        
        public float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; }
        }
        private float nearPlaneDistance = 1.0f;
       
        //get the far clipping plane       
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; }
        }
        private float farPlaneDistance = 100000.0f;
       
        // View transform matrix        
        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }
        private Matrix view;

        
        // Projecton transform matrix        
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }
        private Matrix projection;
        
        // Rebuilds object space values in world space. Invoke before publicly
        // returning or privately accessing world space values      
        private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = chaseCamPosition +
                Vector3.TransformNormal(DesiredPositionOffset, transform);
            lookAt = chaseCamPosition +
                Vector3.TransformNormal(LookAtOffset, transform);
        }
        
        // Rebuilds camera's view and projection matricies        
        private void UpdateMatrices()
        {
            view = Matrix.CreateLookAt(this.Position, this.LookAt, this.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
                AspectRatio, NearPlaneDistance, FarPlaneDistance);
        }
        
        // Forces camera to be at desired position and to stop moving. The is useful
        // when the chased object is first created or after it has been teleported.
        // Failing to call this after a large change to the chased object's position
        // will result in the camera quickly flying across the world        
        public void Reset()
        {
            UpdateWorldPositions();

            // Stop motion
            velocity = Vector3.Zero;

            // Force desired position
            position = desiredPosition;

            UpdateMatrices();
        }
        
        // Animates the camera from its current position towards the desired offset
        // behind the chased object. The camera's animation is controlled by a simple
        // physical spring attached to the camera and anchored to the desired position       
        public void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");
            
            UpdateWorldPositions();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate spring force
            Vector3 stretch = position - desiredPosition;
            Vector3 force = -stiffness * stretch - damping * velocity;

            // Apply acceleration
            Vector3 acceleration = force / mass;
            velocity += acceleration * elapsed;

            // Apply velocity
            position += velocity * elapsed;

            UpdateMatrices();          
        }
    }
}
