namespace EagleEye.Core.ReadModel.EntityFramework.Models
{
    using EagleEye.Core.ReadModel.EntityFramework.Models.Base;

    /// <summary>
    /// Value object.
    /// </summary>
    public class Tag : ValueObjectItemBase
    {
        // Index and is unique set using fluent API.
        public string Value { get; set; }
    }
}
