namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to event metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.EventInfo"/> class.
    /// </remarks>
    public interface IApiEvent : IApiMember
    {
        /// <summary>
        ///     Gets the type of the underlying event-handler delegate associated with this event.
        /// </summary>
        IApiType EventHandlerType { get; }

        /// <summary>
        ///     Gets a value indicating whether the event has accessors.
        /// </summary>
        bool HasAccessors { get; }
    }
}
