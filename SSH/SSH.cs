using Microsoft.CommandPalette.Extensions;
using System.Runtime.InteropServices;

namespace SSH;

[Guid("1d07b190-9870-49a9-8f1d-cf52476e262d")]
public sealed partial class SSH : IExtension, IDisposable
{
	private readonly ManualResetEvent _extensionDisposedEvent;

	private readonly SshCommandsProvider _provider = new();

	public SSH(ManualResetEvent extensionDisposedEvent) => _extensionDisposedEvent = extensionDisposedEvent;

	public object? GetProvider(ProviderType providerType)
	{
		return providerType switch
		{
			ProviderType.Commands => _provider,
			_ => null
		};
	}

	public void Dispose() => _extensionDisposedEvent.Set();
}