namespace Pivot.Framework.Application.Abstractions.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines a saga by providing its type name and ordered list of steps.
///              The saga orchestrator uses this definition to determine step execution
///              order and compensation sequence.
///
///              Usage:
///              <code>
///              public class OrderPaymentSaga : ISagaDefinition&lt;OrderPaymentData&gt;
///              {
///                  public string SagaType => "OrderPayment";
///                  public IReadOnlyList&lt;ISagaStep&lt;OrderPaymentData&gt;&gt; Steps => new[]
///                  {
///                      new CreateOrderIntentStep(),
///                      new CreatePaymentIntentStep(),
///                      new CreateRealOrderStep(),
///                      new ConfirmPaymentStep()
///                  };
///              }
///              </code>
/// </summary>
/// <typeparam name="TSagaData">The saga-specific data type that carries state across steps.</typeparam>
public interface ISagaDefinition<TSagaData> where TSagaData : class
{
	/// <summary>
	/// Gets the unique type name of this saga (e.g., "OrderPayment", "RefundProcess").
	/// Used for persistence and logging.
	/// </summary>
	string SagaType { get; }

	/// <summary>
	/// Gets the ordered list of steps that make up this saga.
	/// Steps are executed in order; compensation runs in reverse order.
	/// </summary>
	IReadOnlyList<ISagaStep<TSagaData>> Steps { get; }
}
