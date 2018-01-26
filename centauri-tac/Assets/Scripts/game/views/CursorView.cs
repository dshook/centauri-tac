using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public enum CursorStyles {
        Default,
        Move,
        Attack,
        Walking
    }

    public class CursorView : View
    {
        Dictionary<CursorStyles, Texture2D> cursorDict;
        Dictionary<CursorStyles, Vector2> cursorOffsets;

        internal void init(IResourceLoaderService loader)
        {
            cursorDict = new Dictionary<CursorStyles, Texture2D>(){
                {CursorStyles.Default, loader.Load<Texture2D>("Images/cursors_default")},
                {CursorStyles.Move, loader.Load<Texture2D>("Images/cursors_move")},
                {CursorStyles.Attack, loader.Load<Texture2D>("Images/cursors_attack")},
                {CursorStyles.Walking, loader.Load<Texture2D>("Images/cursors_walking")}
            };
            var halfCursor = new Vector2(25f, 25f);
            cursorOffsets = new Dictionary<CursorStyles, Vector2>(){
                {CursorStyles.Default, Vector2.zero},
                {CursorStyles.Move, halfCursor},
                {CursorStyles.Attack, halfCursor},
                {CursorStyles.Walking, halfCursor},
            };

            setStyle(CursorStyles.Default);
        }

        void Update()
        {
        }

        internal void setStyle(CursorStyles style)
        {
            Cursor.SetCursor(cursorDict[style], cursorOffsets[style], CursorMode.Auto);
        }

    }
}

