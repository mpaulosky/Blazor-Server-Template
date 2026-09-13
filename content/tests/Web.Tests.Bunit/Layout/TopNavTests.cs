using Bunit;
using BlazorServerTemplate.Web.Components.Layout;

namespace BlazorServerTemplate.Web.Tests.Bunit.Layout;

public class TopNavTests : global::Bunit.BunitContext
{
	[Fact]
	public void Render_WhenAnonymous_ShowsLogInLink()
	{
		// Arrange
		AddAuthorization().SetNotAuthorized();

		// Act
		var cut = Render<TopNav>();

		// Assert
		cut.Markup.Should().Contain("Log in");
	}
}
