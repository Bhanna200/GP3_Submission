using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flight.BepuPhysics
{
    /// <summary>
    /// Component that draws a model.
    /// </summary>
    public class StaticModel : DrawableGameComponent
    {
        Model model;
        public Matrix Transform;
        Matrix[] boneTransforms;

        public StaticModel(Model model, Matrix transform, Game game)
            : base(game)
        {
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
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * Transform;
                    effect.View = (Game as Game1).camera.View;
                    effect.Projection = (Game as Game1).camera.Projection;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
