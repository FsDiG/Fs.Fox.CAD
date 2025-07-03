namespace AcHelper.Commands
{
	/// <summary>
	/// The IAcadCommand interface is used to execute specific AutoCAD commands.
	/// These commands will be executed through the <see cref="CommandHandlerBase.ExecuteCommand{T}"/>ExecuteCommand{T} method.
	/// This way any uncaught error will be caught after all.
	/// </summary>
	public interface IAcadCommand
    {
        /// <summary>
        /// Executes the Command code.
        /// </summary>
        void Execute();
    }
}
