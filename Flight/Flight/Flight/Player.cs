using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities.Prefabs;
using Flight.BepuPhysics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Collidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace Flight
{
    public class Player
    {
        //instantiate game
        public Game game;
        //variable to hold player collision box
        public Box shipColBox;
        //instantiate player model
        public EntityModel shipModel;
        //instantiate lasersound
        public SoundEffect laser;
        // float to hold thrustAmount
        public float thrustAmount = 0;
        //Vector3 to hold ships starting position
        public Vector3 shipPos = new Vector3(0f, 10.0f, 0f);
        //Rotation speed of the player
        private const float RotationRate = 50.5f;
        // Maximum force that can be applied along the ship's direction.        
        private const float ThrustForce = 24.0f;
        // Velocity scalar to approximate drag        
        private const float DragFactor = 0.97f;
        // Current ship velocity        
        public Vector3 Velocity;
        // Location of ship in world space        
        public Vector3 Position;        
        // Direction ship is facing       
        public Vector3 Direction;        
        // Ship's up vector        
        public Vector3 Up;
       
        // Ship's right vector       
        public Vector3 Right
        {
            get { return right; }
        }
        private Vector3 right;
       
        // Ship world transform matrix        
        public Matrix World
        {
            get { return world; }
        }
        private Matrix world;

        //Player Constructor
        public Player(Game1 game)
        {      
            this.game = game;
            //creates a new collision box for the player |location|size X,Y,Z| 
            shipColBox = new Box(shipPos, 1f, 1f, 1f);
            //sets its mass
            shipColBox.Mass = 2.0f;
            // sets if its affected by gravity
            shipColBox.IsAffectedByGravity = false;
            //add collision box to game
            game.space.Add(shipColBox);
            //Loads the ship model and applys transforms
            shipModel = new EntityModel(shipColBox, game.Content.Load<Model>("Models/Ship"), Matrix.Identity * Matrix.CreateScale(0.0005f), game);
            //load laser sound
            laser = game.Content.Load<SoundEffect>("Audio/Laser");
            //add ship model to game
            game.Components.Add(shipModel);
            shipColBox.Tag = shipModel;
            //resets position of player in world space
            //collision rule to prevent the bullet colliding  with the player ship
            //CollisionRules.AddRule(shipColBox, game.bullet, CollisionRule.NoBroadPhase);
            //set bullet collision information with game objects
            shipColBox.CollisionInformation.Events.InitialCollisionDetected += ShipCollision;

        }

        //ship collision detection
        void ShipCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //if the bullet collides with any othe game object the increament the score, play hit sound and remove object from the game
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {                
                //remove object from game                
                game.Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }       

        public void Reset()
        {
            //players position
            Position = shipPos;
            //players direction vector
            Direction = Vector3.Forward;
            //players Up vector
            Up = Vector3.Up;
            //players right vector
            right = Vector3.Right;
            //players velocity
            Velocity = Vector3.Zero;
        }   

        
        // Applies a simple rotation to the ship and animates position based
        // on simple linear motion physics        
        public void Update(GameTime gameTime)
        {

            KeyboardState lastKeyboardState = new KeyboardState();
            KeyboardState currentKeyboardState = new KeyboardState();

            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();           

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            //rotate player left          
            if (lastKeyboardState.IsKeyUp(Keys.Left) && (currentKeyboardState.IsKeyDown(Keys.Left)))
            {                
                shipColBox.AngularVelocity = Vector3.Up;                
            }

            //rotate player right
            if (lastKeyboardState.IsKeyUp(Keys.Right) && (currentKeyboardState.IsKeyDown(Keys.Right)))
            {
                shipColBox.AngularVelocity = Vector3.Down;
            }

            //rotate player Up
            if (lastKeyboardState.IsKeyUp(Keys.Up) && (currentKeyboardState.IsKeyDown(Keys.Up)))
            {
                shipColBox.AngularVelocity = Vector3.Left;
            }

            //rotate player down
            if (lastKeyboardState.IsKeyUp(Keys.Down) && (currentKeyboardState.IsKeyDown(Keys.Down)))
            {
                shipColBox.AngularVelocity = Vector3.Right;
            }

             //Determine thrust amount from input
            if (lastKeyboardState.IsKeyUp(Keys.W) && (currentKeyboardState.IsKeyDown(Keys.W)))
            {
                thrustAmount = 5.0f;            
            }
            //sets thrust and velocity to zero
            else
            {
                thrustAmount = 0;
                shipColBox.LinearVelocity = new Vector3(0, 0, 0f);
            }

            // Scale rotation amount to radians per second
            shipColBox.AngularVelocity = RotationRate * shipColBox.AngularVelocity * elapsed;  

            //Create rotation matrix
            Matrix rotationMatrix = Matrix.CreateRotationY(shipColBox.AngularVelocity.X) *
                Matrix.CreateFromAxisAngle(Right, shipColBox.AngularVelocity.Y);                

            //Rotate orientation vectors
            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            right = Vector3.Cross(Direction, Up);
            
            // re-calculate with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);           

            // Calculate force from thrust amount
            Vector3 force = shipColBox.OrientationMatrix.Forward * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / shipColBox.Mass;
            Velocity += acceleration * elapsed;

            //Apply psuedo drag
            Velocity *= DragFactor;

            //Apply velocity
            shipColBox.Position += Velocity * elapsed;

            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Forward = Direction;
            world.Up = Up;
            world.Right = right;
            world.Translation = Position;
        }
    }
}
