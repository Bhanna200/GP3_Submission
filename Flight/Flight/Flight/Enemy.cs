using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Flight.BepuPhysics;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;

namespace Flight
{
    public class Enemy
    { 
        //instantiate enemy collision box
        public Box enemyColBox;
        //instantiate game
        private Game1 game; 
        //bool to set if enemy is dead or not
        private Boolean dead = false;
        //Seed a new random number
        private Random random = new Random(Guid.NewGuid().GetHashCode());
        //counter to keep track of how many random vector3's
        private static int counter = 0;
        //Vector3 to hold target location
        public Vector3 targetLocation;

        //enemy constructor
        public Enemy(Game1 game)
        {
            this.game = game;
            //creates a new collision box for the enemy |location|size X,Y,Z|is dynamic or not| 
            enemyColBox = new Box(getNewLocation(), 1, 1, 1, 0.1f);
            //is enemy affected by gravity
            enemyColBox.IsAffectedByGravity = false;
            // Model transforms
            Matrix transform = Matrix.CreateScale(enemyColBox.Width, enemyColBox.Height, enemyColBox.Length);
            //Loads the enemy model and places it an the correct position then scales, rotates
            EntityModel enemyModel = new EntityModel(enemyColBox, game.Content.Load<Model>("Models/enemyShip1"), Matrix.Identity * Matrix.CreateScale(0.03f) * Matrix.CreateRotationX(30f), game);
            //adds enemy collision box to the game
            game.space.Add(enemyColBox);
            //adds enemy moddel to the game
            game.Components.Add(enemyModel);
            enemyColBox.Tag = enemyModel;
            //sets the enemy positon at a new random location
            targetLocation = getNewLocation();            
        } 
         
        private Vector3 getNewLocation()
        {
            //holds a tempory vector3 to store each enemys position
            Vector3 tempVector = game.moveLocations[counter];
            //increament counter
            counter++;
            //sets counter back to 0 if counter is greater than the arrays length
            if (counter >= game.moveLocations.Length)
            {
                counter = 0;
            }
            return tempVector;
        }

        public void update(GameTime gameTime)
        {
            if (!dead)
            {
                //if the enemy is within 50 of player set target location as player
                if ((game.player.shipColBox.Position - enemyColBox.Position).Length() < 50) 
                {
                    targetLocation = game.player.shipColBox.Position;                    
                }
                // if the random number generated is less than 0.01 generate a new location for the enemy to move to
                else if (random.NextDouble() < 0.01)
                {                    
                    targetLocation = getNewLocation();
                }
                
                if (enemyColBox.LinearVelocity.Length() < 10)
                {
                    //calculate the new trajectory of the enemy
                    Vector3 enemyTrajectory = targetLocation - enemyColBox.Position; 
                    //set new travel path to a length of 1
                    enemyTrajectory.Normalize();
                    //calculate path from enemy towards player 
                    Vector3 pathToPlayer = new Vector3(enemyTrajectory.X, 0, enemyTrajectory.Z);
                    enemyColBox.LinearVelocity += pathToPlayer;
                }
                // if the enemy position is within 2 of the player position
                if ((enemyColBox.Position - game.player.shipColBox.Position).Length() < 2)
                {
                    // reduce health
                    game.health -= (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;                   
                }
            }
        }

        // bool to return if enemy is dead
        public Boolean isDead()
        {
            return dead;
        }
    }
}
