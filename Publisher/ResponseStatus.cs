namespace Parcsis.PSD.Publisher
{
	public enum ResponseStatus
	{
		/// <summary>
		/// Действие выполнено успешно
		/// </summary>
		Success,

		/// <summary>
		/// Действие выполнено неуспешно
		/// </summary>
		Failure, 

		/// <summary>
		/// Действие выполнено успешно, но данных, удовлетворяющих запросу нет
		/// </summary>
		No_data,

		/// <summary>
		/// Невозможно выполнение, поскольку клиент не авторизирован
		/// </summary>
		Unauthorized, 

		/// <summary>
		/// Внутренняя ошибка сервера
		/// </summary>
		Internal_error
	}
}