using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;
using Flight.BepuPhysics;
using Microsoft.Xna.Framework.Content;
using BEPUphysics.DataStructures;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using BEPUphysics.CollisionRuleManagement;

namespace Flight
{
    public class Enviroment
    {
        
        //The skybox model, which will just be a cube        
        private Model skyBox;
        // variables to hold terrain data
        public Model terrain;
        // variables to hold enemy data
        public Enemy enemy;
        //variable to store turrret positions
        public Vector3 turretPos = new Vector3(50f, 1f, 84f);
        public Vector3 turretPos1 = new Vector3(-55, 1f, -78f);
        public Vector3 turretPos2 = new Vector3(57f, -0.3f, -66f);
        public Vector3 turretPos3 = new Vector3(-57f, -4.2f, 73f);
        //variables to hold turret collision boxes
        public Box turretColBox;
        public Box turretColBox1;
        public Box turretColBox2;
        public Box turretColBox3;
        //variable to store turret model
        public EntityModel turretModel;
        // varibles for buildings
        public Vector3 buildingPos = new Vector3(0f, -1f, 0f);
        public Box buildingColBox;
        public EntityModel buildingModel;

        
        // The actual skybox texture        
        private TextureCube skyBoxTexture;        
        // The effect file that the skybox will use to render        
        private Effect skyBoxEffect;
        
        // The size of the cube, used so that we can resize the box
        // for different sized environments        
        private float size = 155f;

        
        // loads new skybox content      
        public Enviroment(string skyboxTexture, ContentManager Content)
        {           
            skyBox = Content.Load<Model>("Models/skyboxCube");
            skyBoxTexture = Content.Load<TextureCube>("Effects/Sunset");
            skyBoxEffect = Content.Load<Effect>("Effects/Skybox");
            terrain = Content.Load<Model>("Models/desert");
        }

        public void DrawTerrain(Game1 game)
        {
            //===============================TERRAIN================================================
            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(terrain, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -30, 0)));

            //Add it to the space!
            game.space.Add(mesh);
            //Make it visible too.
            game.Components.Add(new StaticModel(terrain, mesh.WorldTransform.Matrix, game));
            //======================================================================================         

        }

        public void DrawTurrets(Game1 game)
        {
            turretColBox = new Box(turretPos, 2f, 2f, 2f);
            turretModel = new EntityModel(turretColBox, game.Content.Load<Model>("Models/turret"), (Matrix.CreateScale(0.020f) * Matrix.CreateRotationX(30f) * Matrix.CreateTranslation(0f,0f,-7f)), game);
            game.space.Add(turretColBox);
            game.Components.Add(turretModel);       
            turretColBox.Tag = turretModel;

            turretColBox1 = new Box(turretPos1, 2f, 2f, 2f);
            turretModel = new EntityModel(turretColBox1, game.Content.Load<Model>("Models/turret"), (Matrix.CreateScale(0.020f) * Matrix.CreateRotationX(30f) * Matrix.CreateTranslation(0f, 0f, -7f)), game);
            game.space.Add(turretColBox1);
            game.Components.Add(turretModel);
            turretColBox1.Tag = turretModel;

            turretColBox2 = new Box(turretPos2, 2f, 2f, 2f);
            turretModel = new EntityModel(turretColBox2, game.Content.Load<Model>("Models/turret"), (Matrix.CreateScale(0.020f) * Matrix.CreateRotationX(30f) * Matrix.CreateTranslation(0f, 0f, -7f)), game);
            game.space.Add(turretColBox2);
            game.Components.Add(turretModel);
            turretColBox2.Tag = turretModel;

            turretColBox3 = new Box(turretPos3, 2f, 2f, 2f);
            turretModel = new EntityModel(turretColBox3, game.Content.Load<Model>("Models/turret"), (Matrix.CreateScale(0.020f) * Matrix.CreateRotationX(30f) * Matrix.CreateTranslation(0f, 0f, -7f)), game);
            game.space.Add(turretColBox3);
            game.Components.Add(turretModel);
            turretColBox3.Tag = turretModel;            
        }

        public void DrawBuidlings(Game1 game)
        {
            buildingColBox = new Box(buildingPos, 6f, 3f, 3f);
            buildingModel = new EntityModel(buildingColBox, game.Content.Load<Model>("Models/shack"), Matrix.Identity * (Matrix.CreateScale(0.05f)*Matrix.CreateRotationX(30.0f)), game);
            game.space.Add(buildingColBox);
            game.Components.Add(buildingModel);
            buildingColBox.Tag = buildingModel;            
        }
       
        // Does the actual drawing of the skybox with our skybox effect.
        // There is no world matrix, because we're assuming the skybox won't
        // be moved around.  The size of the skybox can be changed with the size
        // variable.        
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            // Go through each pass in the effect, but we know there is only one...
            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                // Draw all of the components of the mesh, but we know the cube really
                // only has one mesh
                foreach (ModelMesh mesh in skyBox.Meshes)
                {
                    // Assign the appropriate values to each of the parameters
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(Matrix.CreateScale(size));
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }

                    // Draw the mesh with the skybox effect
                    mesh.Draw();
                }
            }
        }
    }
}
