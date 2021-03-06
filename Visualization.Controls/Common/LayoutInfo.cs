﻿using System;
using System.Windows;

namespace Visualization.Controls.Common
{
    [Serializable]
    public abstract class LayoutInfo
    {
        public abstract void Backup();
        public abstract bool IsHit(Point mousePos);
        public abstract void Rollback();
    }
}