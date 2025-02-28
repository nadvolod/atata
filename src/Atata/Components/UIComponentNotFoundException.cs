﻿using System;
using System.Runtime.Serialization;

namespace Atata
{
    [Serializable]
    public class UIComponentNotFoundException : Exception
    {
        public UIComponentNotFoundException()
        {
        }

        public UIComponentNotFoundException(string message)
            : base(message)
        {
        }

        public UIComponentNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected UIComponentNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static UIComponentNotFoundException ForParentOf(string componentFullName) =>
            ForAncestor(componentFullName, "parent");

        public static UIComponentNotFoundException ForGrandparentOf(string componentFullName) =>
            ForAncestor(componentFullName, "grandparent");

        public static UIComponentNotFoundException ForGreatGrandparent(string componentFullName) =>
            ForAncestor(componentFullName, "great grandparent");

        private static UIComponentNotFoundException ForAncestor(string componentFullName, string ancestorName) =>
            new UIComponentNotFoundException($"Failed to find {ancestorName} component of {componentFullName}.");
    }
}
