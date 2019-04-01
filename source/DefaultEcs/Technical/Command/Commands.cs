﻿using System.Runtime.InteropServices;

namespace DefaultEcs.Technical.Command
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct EntityCommand
    {
        [FieldOffset(0)]
        public CommandType CommandType;

        [FieldOffset(1)]
        public Entity Entity;

        public EntityCommand(CommandType commandType, Entity entity)
        {
            CommandType = commandType;
            Entity = entity;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct EntityOffsetCommand
    {
        [FieldOffset(0)]
        public readonly CommandType CommandType;

        [FieldOffset(1)]
        public readonly int EntityOffset;

        public EntityOffsetCommand(CommandType commandType, int entityOffset)
        {
            CommandType = commandType;
            EntityOffset = entityOffset;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ChildParentOffsetCommand
    {
        [FieldOffset(0)]
        public readonly CommandType CommandType;

        [FieldOffset(1)]
        public readonly int ChildOffset;

        [FieldOffset(5)]
        public readonly int ParentOffset;

        public ChildParentOffsetCommand(CommandType commandType, int childOffset, int parentOffset)
        {
            CommandType = commandType;
            ChildOffset = childOffset;
            ParentOffset = parentOffset;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct EntityOffsetComponentCommand
    {
        [FieldOffset(0)]
        public readonly CommandType CommandType;

        [FieldOffset(1)]
        public readonly int ComponentIndex;

        [FieldOffset(5)]
        public readonly int EntityOffset;

        public EntityOffsetComponentCommand(CommandType commandType, int componentIndex, int entityOffset)
        {
            CommandType = commandType;
            ComponentIndex = componentIndex;
            EntityOffset = entityOffset;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct EntityReferenceOffsetComponentCommand
    {
        [FieldOffset(0)]
        public readonly CommandType CommandType;

        [FieldOffset(1)]
        public readonly int ComponentIndex;

        [FieldOffset(5)]
        public readonly int EntityOffset;

        [FieldOffset(9)]
        public readonly int ReferenceOffset;

        public EntityReferenceOffsetComponentCommand(CommandType commandType, int componentIndex, int entityOffset, int referenceOffset)
        {
            CommandType = commandType;
            ComponentIndex = componentIndex;
            EntityOffset = entityOffset;
            ReferenceOffset = referenceOffset;
        }
    }
}
