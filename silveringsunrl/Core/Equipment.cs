using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RogueSharp;
using SilveringSunRL.Equipment;
using SilveringSunRL.Interfaces;
using Console = SadConsole.Console;

namespace SilveringSunRL.Core
{
    public class Equipment : IEquipment, ITreasure, Interfaces.IDrawable
    {
        public Equipment()
        {
            Symbol = ']';
            Color = Color.Yellow;
        }

        public int Attack { get; set; }
        public int AttackChance { get; set; }
        public int Awareness { get; set; }
        public int Defense { get; set; }
        public int DefenseChance { get; set; }
        public int Gold { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }

        protected bool Equals(Equipment other)
        {
            return Attack == other.Attack && AttackChance == other.AttackChance && Awareness == other.Awareness && Defense == other.Defense && DefenseChance == other.DefenseChance && Gold == other.Gold && Health == other.Health && MaxHealth == other.MaxHealth && string.Equals(Name, other.Name) && Speed == other.Speed;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Equipment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Attack;
                hashCode = (hashCode * 397) ^ AttackChance;
                hashCode = (hashCode * 397) ^ Awareness;
                hashCode = (hashCode * 397) ^ Defense;
                hashCode = (hashCode * 397) ^ DefenseChance;
                hashCode = (hashCode * 397) ^ Gold;
                hashCode = (hashCode * 397) ^ Health;
                hashCode = (hashCode * 397) ^ MaxHealth;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Speed;
                return hashCode;
            }
        }

        public static bool operator ==(Equipment left, Equipment right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Equipment left, Equipment right)
        {
            return !Equals(left, right);
        }

        public bool PickUp(IActor actor)
        {
            if (this is HeadEquipment)
            {
                actor.Head = this as HeadEquipment;
                GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up a {Name} helmet");
                return true;
            }

            if (this is BodyEquipment)
            {
                actor.Body = this as BodyEquipment;
                GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up {Name} body armor");
                return true;
            }

            if (this is HandEquipment)
            {
                actor.Hand = this as HandEquipment;
                GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up a {Name}");
                return true;
            }

            if (this is FeetEquipment)
            {
                actor.Feet = this as FeetEquipment;
                GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up {Name} boots");
                return true;
            }

            return false;
        }

        public Color Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public void Draw(Console console, IMap map)
        {
            if (!map.IsExplored(X, Y))
            {
                return;
            }

            if (map.IsInFov(X, Y))
            {
                //console.CellData.SetCharacter(X, Y, Symbol, Color, Colors.FloorBackgroundFov);

                int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = Symbol;
                GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Color;
                GameLoop.World.CurrentMap.Tiles[tilePos].Background = Colors.FloorBackgroundFov;
            }
            else
            {
                //console.CellData.SetCharacter(X, Y, Symbol, Color.Multiply(Color.Gray, 0.5f), Colors.FloorBackground);

                int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = Symbol;
                GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Color.Multiply(Color.Gray, 0.5f);
                GameLoop.World.CurrentMap.Tiles[tilePos].Background = Colors.FloorBackground;
            }
        }
    }
}
