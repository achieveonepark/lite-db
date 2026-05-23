using System;

namespace SQLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class AutoIncrementAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}
