﻿using System.Drawing;

namespace DrawAnce
{
    public interface IDrawGen
    {
        IndiWrap[] AncData { get; set; }

        Image MakeAncTree();

        int HitIndex(Point hit);

    }

    public abstract class DrawGen : IDrawGen
    {
        protected const string MORE_GEN = "►";
        protected const int MoreGenW = 25;

        protected Rectangle[] _hitRect;

        public IndiWrap[] AncData { get; set; }
        public abstract Image MakeAncTree();

        protected void Init()
        {
            _hitRect = new Rectangle[32];
            for (int i = 0; i < 32; i++)
                _hitRect[i] = new Rectangle();
        }

        /// <summary>
        /// Determine which rectangle a Point intersects.
        /// </summary>
        /// <param name="hit"></param>
        /// <returns>The index within the rectangle array; -1 if no intersection.</returns>
        public int HitIndex(Point hit)
        {
            if (_hitRect == null)
                return -1;
            for (int i = 0; i < _hitRect.Length; i++)
                if (_hitRect[i].Contains(hit))
                    return i;
            return -1;
        }

    }
}
