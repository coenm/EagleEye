namespace EagleEye.Core.ReadModel.EntityFramework.Models.Base
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public abstract class ValueObjectItemBase
    {
        [Key]
        public Guid Id { get; set; }
    }
}