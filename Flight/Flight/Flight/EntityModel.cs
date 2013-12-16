using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Flight
{
    public class EntityModel : DrawableGameComponent
    {

        Entity entity;
        Model model;

        public Matrix Transform;
        Matrix[] boneTransforms;

        public EntityModel(Entity entity, Model model, Matrix transform, Game game)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            this.Transform = transform;

            boneTransforms = new Matrix[model.Bones.Count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix worldMatrix = Transform * entity.WorldTransform;


            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix;
                    effect.View = (Game as Game1).camera.View;
                    effect.Projection = (Game as Game1).camera.Projection;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
