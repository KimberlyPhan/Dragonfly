// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: tensorflow/core/framework/tensor_description.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tensorflow {

  /// <summary>Holder for reflection information generated from tensorflow/core/framework/tensor_description.proto</summary>
  public static partial class TensorDescriptionReflection {

    #region Descriptor
    /// <summary>File descriptor for tensorflow/core/framework/tensor_description.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TensorDescriptionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjJ0ZW5zb3JmbG93L2NvcmUvZnJhbWV3b3JrL3RlbnNvcl9kZXNjcmlwdGlv",
            "bi5wcm90bxIKdGVuc29yZmxvdxoldGVuc29yZmxvdy9jb3JlL2ZyYW1ld29y",
            "ay90eXBlcy5wcm90bxosdGVuc29yZmxvdy9jb3JlL2ZyYW1ld29yay90ZW5z",
            "b3Jfc2hhcGUucHJvdG8aNnRlbnNvcmZsb3cvY29yZS9mcmFtZXdvcmsvYWxs",
            "b2NhdGlvbl9kZXNjcmlwdGlvbi5wcm90byKoAQoRVGVuc29yRGVzY3JpcHRp",
            "b24SIwoFZHR5cGUYASABKA4yFC50ZW5zb3JmbG93LkRhdGFUeXBlEisKBXNo",
            "YXBlGAIgASgLMhwudGVuc29yZmxvdy5UZW5zb3JTaGFwZVByb3RvEkEKFmFs",
            "bG9jYXRpb25fZGVzY3JpcHRpb24YBCABKAsyIS50ZW5zb3JmbG93LkFsbG9j",
            "YXRpb25EZXNjcmlwdGlvbkI4ChhvcmcudGVuc29yZmxvdy5mcmFtZXdvcmtC",
            "F1RlbnNvckRlc2NyaXB0aW9uUHJvdG9zUAH4AQFiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Tensorflow.TypesReflection.Descriptor, global::Tensorflow.TensorShapeReflection.Descriptor, global::Tensorflow.AllocationDescriptionReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.TensorDescription), global::Tensorflow.TensorDescription.Parser, new[]{ "Dtype", "Shape", "AllocationDescription" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TensorDescription : pb::IMessage<TensorDescription> {
    private static readonly pb::MessageParser<TensorDescription> _parser = new pb::MessageParser<TensorDescription>(() => new TensorDescription());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TensorDescription> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.TensorDescriptionReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TensorDescription() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TensorDescription(TensorDescription other) : this() {
      dtype_ = other.dtype_;
      Shape = other.shape_ != null ? other.Shape.Clone() : null;
      AllocationDescription = other.allocationDescription_ != null ? other.AllocationDescription.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TensorDescription Clone() {
      return new TensorDescription(this);
    }

    /// <summary>Field number for the "dtype" field.</summary>
    public const int DtypeFieldNumber = 1;
    private global::Tensorflow.DataType dtype_ = 0;
    /// <summary>
    /// Data type of tensor elements
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.DataType Dtype {
      get { return dtype_; }
      set {
        dtype_ = value;
      }
    }

    /// <summary>Field number for the "shape" field.</summary>
    public const int ShapeFieldNumber = 2;
    private global::Tensorflow.TensorShapeProto shape_;
    /// <summary>
    /// Shape of the tensor.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.TensorShapeProto Shape {
      get { return shape_; }
      set {
        shape_ = value;
      }
    }

    /// <summary>Field number for the "allocation_description" field.</summary>
    public const int AllocationDescriptionFieldNumber = 4;
    private global::Tensorflow.AllocationDescription allocationDescription_;
    /// <summary>
    /// Information about the size and allocator used for the data
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.AllocationDescription AllocationDescription {
      get { return allocationDescription_; }
      set {
        allocationDescription_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TensorDescription);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TensorDescription other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Dtype != other.Dtype) return false;
      if (!object.Equals(Shape, other.Shape)) return false;
      if (!object.Equals(AllocationDescription, other.AllocationDescription)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Dtype != 0) hash ^= Dtype.GetHashCode();
      if (shape_ != null) hash ^= Shape.GetHashCode();
      if (allocationDescription_ != null) hash ^= AllocationDescription.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Dtype != 0) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Dtype);
      }
      if (shape_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Shape);
      }
      if (allocationDescription_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(AllocationDescription);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Dtype != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Dtype);
      }
      if (shape_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Shape);
      }
      if (allocationDescription_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(AllocationDescription);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TensorDescription other) {
      if (other == null) {
        return;
      }
      if (other.Dtype != 0) {
        Dtype = other.Dtype;
      }
      if (other.shape_ != null) {
        if (shape_ == null) {
          shape_ = new global::Tensorflow.TensorShapeProto();
        }
        Shape.MergeFrom(other.Shape);
      }
      if (other.allocationDescription_ != null) {
        if (allocationDescription_ == null) {
          allocationDescription_ = new global::Tensorflow.AllocationDescription();
        }
        AllocationDescription.MergeFrom(other.AllocationDescription);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            dtype_ = (global::Tensorflow.DataType) input.ReadEnum();
            break;
          }
          case 18: {
            if (shape_ == null) {
              shape_ = new global::Tensorflow.TensorShapeProto();
            }
            input.ReadMessage(shape_);
            break;
          }
          case 34: {
            if (allocationDescription_ == null) {
              allocationDescription_ = new global::Tensorflow.AllocationDescription();
            }
            input.ReadMessage(allocationDescription_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
