// using System;
// public struct ByteBool : IEquatable<ByteBool>
// {
//     private byte value;
//     // Construct
//     public ByteBool(bool Value)
//     {
//         this.value = (byte)(Value ? 1: 0);
//     }
//     // Properties
//     public bool Value
//     {
//         // why use != not == ? Is there any possibility to set other numbers?
//         get { return this.value != 0; }
//         set { this.value = (byte)(Value ? 1 : 0); }
//     }
//     // Equal 
//     public bool Equals( ByteBool other)
//     {
//         return other.value == value;
//     }

//     public override bool Equals(object obj) 
//     {
//         if (ReferenceEquals(null, obj)) 
//         {
//             return false;
//         }
//         return obj is ByteBool && Equals((ByteBool)obj);
//     }

//     public override int GetHashCode() {
//         return this.value.GetHashCode();
//     }

//     /// <summary>
//     /// Converts a bool to a ByteBool
//     /// </summary>
//     /// 
//     /// 
//     public static implicit operator ByteBool(bool value) {
//         return new ByteBool(value);
//     }

//     /// <summary>
//     /// Converts a ByteBool to a bool
//     /// </summary>
//     /// 
//     /// 
//     public static implicit operator bool(ByteBool source) {
//         return source.Value;
//     }
// }

