namespace MpSoft.AspNetCore.AggregatedServices
{
	public class JellequinAggregatedServiceGeneratorException:AggregatedServiceGeneratorException
	{
		readonly string _message;
		readonly JellequinAggregatedServiceGeneratorExceptionReason _reason;
		internal JellequinAggregatedServiceGeneratorException(JellequinAggregatedServiceGeneratorExceptionReason reason)
		{
			_reason=reason;
			switch (_reason)
			{
				case JellequinAggregatedServiceGeneratorExceptionReason.InvalidOptions:
					_message="Invalid options.";
					break;
				case JellequinAggregatedServiceGeneratorExceptionReason.ServiceTypeMustBeInterface:
					_message="Service type must be a public interface.";
					break;
				case JellequinAggregatedServiceGeneratorExceptionReason.InvalidMemberType:
					_message="Service interfaces must contains only properties consist of getter.";
					break;
				case JellequinAggregatedServiceGeneratorExceptionReason.PropertiesMustBeUnique:
					_message="Property names across all interfaces must be unique.";
					break;
				default:
					_message="Unknown issue.";
					break;
			}
		}

		public JellequinAggregatedServiceGeneratorExceptionReason Reason => _reason;

		public override string Message => _message;
	}

	public enum JellequinAggregatedServiceGeneratorExceptionReason { InvalidOptions, ServiceTypeMustBeInterface, InvalidMemberType, PropertiesMustBeUnique }
}
