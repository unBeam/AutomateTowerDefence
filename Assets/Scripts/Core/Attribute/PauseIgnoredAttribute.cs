using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class PauseIgnoredAttribute : Attribute {}