/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Utilities
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathRun
{

    using UnityEngine;

    /**
     * Transformations that can be inexpensively copied (since the = operator in c# copies the reference not the object, and
     * copying a "real" Transform requires instantiating a game object: blarg)
     */
    public class Trans
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public Trans(Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
        {
            position = newPosition;
            rotation = newRotation;
            localScale = newLocalScale;
        }

        public Trans()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
        }

        public Trans(Transform transform)
        {
            copyFrom(transform);
        }

        public void copyFrom(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        public void copyFrom(Trans trans)
        {
            position = trans.position;
            rotation = trans.rotation;
            localScale = trans.localScale;
        }

        public void copyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }

        public void copyTo(Trans trans)
        {
            trans.position = position;
            trans.rotation = rotation;
            trans.localScale = localScale;
        }
    }
}