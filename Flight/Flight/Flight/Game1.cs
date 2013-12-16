using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysicsDrawer.Lines;
using BEPUphysics.Entities.Prefabs;
using Flight.BepuPhysics;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Collidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace Flight
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //instantiate enviroment
        private Enviroment enviroment;
        //instantiate space
        public Space space;
        //instantiate player
        public Player player;
        //instantiate enemy
        public Enemy enemy;
        //instantiate camera
        public ChaseCamera camera;
        //instantiate bullet
        public Box bullet;
        //instantiate graphics
        private GraphicsDeviceManager graphics;        
        private SpriteBatch spriteBatch;
        public SpriteFont font;
        private Texture2D greenHealthBar, redHealthBar, healthText, titleScreen, howToScreen, getReadyScreen, endGameScreen, cockpit;
        private int screenWidth, screenHeight, screen = 0;         
        //list for enemies
        private List<Enemy> deadEnemies = new List<Enemy>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Enemy> newEnemies = new List<Enemy>();
        //float to hold the the size of the terrain
        public float terrainSize = 400;
        //float to store player health value
        public float health = 30;
        //player score variable
        public int score = 0;
        //array of vector3 movelocations
        public Vector3[] moveLocations;
        //booleans to hold game stats and camera states
        private Boolean started = false, dead = false, cameraModeFirstPerson = false, cameraModeChase = true,
                        cameraSpringEnabled = true, debug = false, soundFx = false, playMusic = false;     
        //Vector3 to hold camera position
        private Vector3 cameraPosition;
        ///instantiate keyboard states
        private KeyboardState lastKeyboardState = new KeyboardState();
        private KeyboardState currentKeyboardState = new KeyboardState();
        //instantiate debug
        private BoundingBoxDrawer debugBoundingBoxDrawer;
        private BasicEffect debugDrawer;
        //variables to store sound effects
        private SoundEffect laser;
        protected Song music;
        public SoundEffect hit;
        //instantiate Particle effects
        public ParticleSystem fireParticles;
        // Random number generator for the fire effect.
        public Random random = new Random();

        public Game1()
        {
            //Initialise the graphics
            graphics = new GraphicsDeviceManager(this);            
            screenWidth = 1200;
            screenHeight = 720;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            //Initialise the Cameras
            camera = new ChaseCamera();

            //Initialise the values
            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 100000.0f;

            //set the contenet directory
            Content.RootDirectory = "Content";

            fireParticles = new FireParticleSystem(this, Content);
            fireParticles.DrawOrder = 500;
            Components.Add(fireParticles);
        }

        protected override void Initialize()
        {
            //create the ame space
            space = new Space();

            //set the space gravity
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //create the player
            player = new Player(this);

            //call UpdateCameraChaseTarget method
            UpdateCameraChaseTarget();

            //call reset camera
            camera.Reset();

            //create the bounding box for the debug view
            debugBoundingBoxDrawer = new BoundingBoxDrawer(this);
            debugDrawer = new BasicEffect(GraphicsDevice);

            //An array to 100 new vector3 move locations
            moveLocations = new Vector3[100];

            //generates a new random number
            Random random = new Random(Guid.NewGuid().GetHashCode());

            //for each movelocation in the array calculates a new random vector3 in the game for the placement of enemies
            for (int i = 0; i < moveLocations.Length; i++)
            {
                float x = random.Next((int)terrainSize) - terrainSize / 2f;
                float y = 15f;
                float z = random.Next((int)terrainSize) - terrainSize / 2f;
                moveLocations[i] = new Vector3(x, y, z);
            }
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //load sound effects
            laser = Content.Load<SoundEffect>("Audio/Laser");
            hit = Content.Load<SoundEffect>("Audio/Hit");
            music = Content.Load<Song>("Audio/background");
            
            //load font
            font = Content.Load<SpriteFont>("Fonts/gameFont");

            //load textures
            endGameScreen = Content.Load<Texture2D>("Textures/deathSceen");
            titleScreen = Content.Load<Texture2D>("Textures/titleScreen");
            howToScreen = Content.Load<Texture2D>("Textures/howToScreen");
            getReadyScreen = Content.Load<Texture2D>("Textures/getReadyScreen");
            cockpit = Content.Load<Texture2D>("Textures/cockpit");
            
            //load health bar
            healthText = Content.Load<Texture2D>("Textures/health");
            greenHealthBar = new Texture2D(GraphicsDevice, 1, 1);
            greenHealthBar.SetData(new[] { Color.Green });
            redHealthBar = new Texture2D(GraphicsDevice, 1, 1);
            redHealthBar.SetData(new[] { Color.LightBlue });
           
            //load enviroment
            enviroment = new Enviroment("Skyboxes/Sunset", Content);
            enviroment.DrawTerrain(this);
            enviroment.DrawTurrets(this);            
            enviroment.DrawBuidlings(this);       

            //load enemies variable can be change to more or less
            makeEnemies(500);
            
            //Bools to handle if music
            if (playMusic == true)
            {
                MediaPlayer.Play(music);
            }

            if (playMusic == false)
            {
                MediaPlayer.Stop();
            }            
        }

        protected override void UnloadContent()
        {
        }

        private void UpdateCameraChaseTarget()
        {
            //if the camera mode is set to true then set the following values
            if (cameraModeChase == true)
            {
                //sets the offset of the camera 
                camera.DesiredPositionOffset = new Vector3(0.0f, 10.0f, 10.0f);
                //sets the loock at offset
                camera.LookAtOffset = new Vector3(0.0f, 3.0f, 0.0f);
                //sets camera spring
                cameraSpringEnabled = true;
                //sets the camera position to be the same vector 3 as the player position
                camera.chaseCamPosition = player.shipColBox.Position;
                //sets the camera direction to equal the forward direction of the player
                camera.ChaseDirection = player.shipColBox.OrientationMatrix.Forward;
                //sets the up vector of the camera to equal the players up vector
                camera.Up = player.shipColBox.OrientationMatrix.Up;
            }

            //values are changed from cameraModeChase to move the postion and offset of the camera
            if (cameraModeFirstPerson == true)
            {
                camera.DesiredPositionOffset = new Vector3(0.0f, 1.0f, 1.0f);
                camera.LookAtOffset = new Vector3(0.0f, 1.0f, 0.0f);
                cameraModeChase = false;
                cameraSpringEnabled = false;                
                camera.chaseCamPosition = player.shipColBox.Position;
                camera.ChaseDirection = player.shipColBox.OrientationMatrix.Forward;
                camera.Up = player.shipColBox.OrientationMatrix.Up;               
            }
        }

        protected override void Update(GameTime gameTime)
        {
            //Steps the simulation forward one time step.
            space.Update();
            
            //gets the current and last keyboard state
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();        
                       
            //exit game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // Pressing the A button or key toggles the spring behavior on and off
            if (lastKeyboardState.IsKeyUp(Keys.A) && (currentKeyboardState.IsKeyDown(Keys.A)))
            {
                cameraSpringEnabled = !cameraSpringEnabled;
            }

            //Fire Bullet
            if (lastKeyboardState.IsKeyDown(Keys.Space)) 
            {
                Firebullet();
            }
            //turn off sound FX
            if (lastKeyboardState.IsKeyDown(Keys.Delete) && (currentKeyboardState.IsKeyUp(Keys.Delete)))
            {
                soundFx = false;
            }
            //turn on soundFx
            if (lastKeyboardState.IsKeyDown(Keys.Insert) && (currentKeyboardState.IsKeyUp(Keys.Insert)))
            {
                soundFx = true;
            }
            //turn off music
            if (lastKeyboardState.IsKeyDown(Keys.F11) && (currentKeyboardState.IsKeyUp(Keys.F11)))
            {
                playMusic = false;
                MediaPlayer.Stop();
            }
            //turn on mucic
            if (lastKeyboardState.IsKeyDown(Keys.F12) && (currentKeyboardState.IsKeyUp(Keys.F12)))
            {
                playMusic = true;
                MediaPlayer.Play(music);
            }
            //camera mode is chase camera
            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                cameraModeChase = true;
                cameraModeFirstPerson = false;
            }
            //camera mode is first person 
            if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                cameraModeChase = false;
                cameraModeFirstPerson = true;
            }          

            // The chase cameras update behavior is the spring but we can
            // use the Reset method to have a locked, spring-less camera
            if (cameraSpringEnabled)
                camera.Update(gameTime);
            else
                camera.Reset();
            //hold button to show Debug
            if (currentKeyboardState.IsKeyDown(Keys.U))
            {
                debug = true;
            }
            else debug = false;
            //if player heals becomes less than zer than set gameState to false
            if (health < 0)
            {
                dead = true;
            }
            //if the game has started and the player is not dead
            if (started && !dead)
            {
                //update the space
                space.Update();
                //call update camera
                UpdateCameraChaseTarget();                
                //call the player update method
                player.Update(gameTime);                
                //for each enemy in the list check if its dead and if it is add it to the list of dead enemys
                foreach (Enemy e in enemies)
                {
                    e.update(gameTime);
                    if (e.isDead())
                    {
                        deadEnemies.Add(e);
                    }
                }
                //for each enemy in the list remove enemys for the list
                foreach (Enemy e in deadEnemies)
                {
                    enemies.Remove(e);
                }
                //for each enemy in the add enemey to list of enemies
                foreach (Enemy e in newEnemies)
                {
                    enemies.Add(e);
                }
                //clear list of enemys
                newEnemies.Clear();
            }
            
            //if the game has starte and eneter has been pressesd then call the advance method
            if (!started)
                if (lastKeyboardState.IsKeyDown(Keys.Enter) && (currentKeyboardState.IsKeyUp(Keys.Enter)))
                {
                    advance();
                    
                }
            UpdateFire();
            
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;

            //Draws the enviroment
            enviroment.Draw(camera.View, camera.Projection, cameraPosition);
            fireParticles.SetCamera(camera.View, camera.Projection);
            base.Draw(gameTime);

            //method calls to draw screens and player cockpit to the game screen
            DrawFirstPersonCamera();
            DrawDebug();
            DrawSpashScreens();
            ToggleSoundText();
            ToggleMusicText();

            

            //if the game has started call the DrawHealthBar Method
            if (started)
            {
                DrawHealthBar();
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        //draws splash screens
        private void DrawSpashScreens()
        {
            spriteBatch.Begin();
            //if the screen value = 0 then draw title screen
            if (screen == 0)
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            //if the screen value = 0 then draw howToScreen
            if (screen == 1)
                spriteBatch.Draw(howToScreen, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            //if the screen value = 0 then draw getReady screen
            if (screen == 2)
                spriteBatch.Draw(getReadyScreen, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            //if the screen value = 0 then draw deathScreen screen
            if (dead)
                spriteBatch.Draw(endGameScreen, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();
        }

        //draws the player health bar
        private void DrawHealthBar()
        {
            spriteBatch.Begin();
            //draws the red healthbar back ground texture to the screen 
            spriteBatch.Draw(redHealthBar, new Vector2(0, screenHeight - 20), null, Color.Red, 0f, Vector2.Zero, new Vector2(screenWidth, 20), SpriteEffects.None, 0f);
            //draws the green healthbar back ground texture to the screen
            spriteBatch.Draw(greenHealthBar, new Vector2(0, screenHeight - 20), null, Color.LightGreen, 0f, Vector2.Zero, new Vector2(screenWidth * health / 30, 40), SpriteEffects.None, 0f);
            //draws the heath texture font to the screen
            spriteBatch.Draw(healthText, new Vector2(screenWidth / 2 - 50, screenHeight - 30), Color.White);
            //draws the players score text to the screen and places the players score counter next to it
            spriteBatch.DrawString(font, "SCORE: " + score, new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }

        //draws debug information
        private void DrawDebug()
        {
            if (debug == true)
            {
                spriteBatch.Begin();
                debugDrawer.LightingEnabled = false;
                debugDrawer.VertexColorEnabled = true;
                debugDrawer.World = Matrix.Identity;
                debugDrawer.View = camera.View;
                debugDrawer.Projection = camera.Projection;
                debugBoundingBoxDrawer.Draw(debugDrawer, space);
                spriteBatch.DrawString(font, "Ship Pos: " + player.shipColBox.Position, new Vector2(10, 10), Color.Black);
                spriteBatch.End();
            }
        }

        //method to create enemys 
        public void makeEnemies(int num)
        {
            for (int x = 0; x < num; x++)
            {
                newEnemies.Add(new Enemy(this));
            }
        }
        //create and fire bullet
        public void Firebullet()
        {
            //if sound fx value is true the play laser sound
            if (soundFx == true)
            {
                laser.Play();
            }
            //instantiate bullet collision box
            bullet = new Box(player.shipColBox.Position, 0.2f, 0.2f, 0.2f, 1f);
            //set the bullets LinearVelocity
            bullet.LinearVelocity = player.shipColBox.OrientationMatrix.Forward * 150;
            //set the bullets orientation to the players orientation
            bullet.Orientation = player.shipColBox.Orientation;
            //and the bullet to the game space
            space.Add(bullet);
            //load bullet model and carry out its transforms
            EntityModel model = new EntityModel(bullet, Content.Load<Model>("Models/rocket"), Matrix.Identity * Matrix.CreateScale(0.1f) * Matrix.CreateRotationY(34.6f), this);
            //add bullets model to the game space
            Components.Add(model);
            bullet.Tag = model;
            //collision rule to prevent the bullet coliding with the player ship
            CollisionRules.AddRule(player.shipColBox, bullet, CollisionRule.NoBroadPhase);
            //set bullet collision information with game objects
            bullet.CollisionInformation.Events.InitialCollisionDetected += BulletCollision;
        }

        //bulllet collision detection
        void BulletCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //if the bullet collides with any othe game object the increament the score, play hit sound and remove object from the game
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                //increament score
                score++;
                //player hit sound
                hit.Play();                
                //remove object from game
                space.Remove(otherEntityInformation.Entity);
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }       
        //draw sound fx text 
        private void ToggleSoundText()
        {
            //if the soundFx is true then draw text to screen
            if (soundFx == true && lastKeyboardState.IsKeyDown(Keys.Delete))
            {
                spriteBatch.Begin();
                //draw "SoundFX On: " in center of the screen and set colour to green/yellow
                spriteBatch.DrawString(font, "SoundFX Off: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
            //if the sound effect is false the draw text to screen
            if (soundFx == false && lastKeyboardState.IsKeyDown(Keys.Insert))
            {
                spriteBatch.Begin();
                //draw "SoundFX On: " in center of the screen and set colour to green/yellow
                spriteBatch.DrawString(font, "SoundFX On: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
        }
        //draw music on text 
        private void ToggleMusicText()
        {
            //if the playMusic is true then draw text to screen
            if (playMusic == true && lastKeyboardState.IsKeyDown(Keys.F11))
            {
                spriteBatch.Begin();
                //draw "Music Off: " in center of the screen and set colour to green/yellow
                spriteBatch.DrawString(font, "Music Off: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
            //if the playMusic is false then draw text to screen
            if (playMusic == false && lastKeyboardState.IsKeyDown(Keys.F12))
            {
                spriteBatch.Begin();
                //draw "Music On: " in center of the screen and set colour to green/yellow
                spriteBatch.DrawString(font, "Music On: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
        }
        //Draws the cockpit
        private void DrawScenery()
        {
            //creates a rectangle dor the cockpit texture to be drawn the samesize as the screen window
            Rectangle screenRectangle = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            //draws the cockpit texture to the screen
            spriteBatch.Draw(cockpit, screenRectangle, Color.White);
        }
        //Draw Draw3rdPersonCamera
        private void DrawFirstPersonCamera()
        {
            //if the cameraModeFirstPerson is true then call the DrawScenery() methode
            if (cameraModeFirstPerson == true)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DrawScenery();
                spriteBatch.End();
            }
        }        
        //advance screens method
        private void advance()
        {
            //increament screen
            screen++;
            //if the screen counter is equal to three then then play music, sound effects and set game started to true
            if (screen == 3)
            {
                MediaPlayer.Play(music);
                soundFx = true;            
                started = true;
            }            
        }

        void UpdateFire()
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
            }            
        }

        Vector3 RandomPointOnCircle()
        {
            const float radius = 20;
            const float height = 30;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }
    }
}
