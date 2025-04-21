﻿using servers_api.api.streaming.connectionContexts;
using servers_api.messaging.sending.abstractions;
using servers_api.messaging.sending.main;

public class MessageSender : IMessageSender
{
	private readonly ConnectionMessageSenderFactory _senderFactory;

	public MessageSender(ConnectionMessageSenderFactory senderFactory)
	{
		_senderFactory = senderFactory;
	}

	public async Task SendMessagesToClientAsync(
		IConnectionContext connectionContext,
		string queueForListening,
		CancellationToken cancellationToken)
	{
		var sender = _senderFactory.CreateSender(connectionContext);
		await sender.SendMessageAsync(queueForListening, cancellationToken);
	}
}
