using System;


namespace JetBrains.Annotations
{
	/// <summary>
	/// This attribute is intended to mark publicly available types which should not be removed and so is treated as used.
	/// </summary>
	[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
	public sealed class PublicTypeAttribute : Attribute
	{
		public PublicTypeAttribute() { }
		public PublicTypeAttribute(string comment) { }
	}
}