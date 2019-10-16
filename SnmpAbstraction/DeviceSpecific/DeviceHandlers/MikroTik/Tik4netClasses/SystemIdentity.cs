namespace tik4net.Objects.System
{
	/// <summary>
	/// /system/identity/print: 
	/// </summary>
	[TikEntity("/system/identity")]
	public class SystemIdentity
	{
		/// <summary>
		/// Gets or sets the name of the system.
		/// </summary>
		[TikProperty("name", IsMandatory = true)]
		public string Name { get; set; }
	}
}