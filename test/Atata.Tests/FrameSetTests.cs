﻿using NUnit.Framework;

namespace Atata.Tests
{
    [Category("Unstable")]
    public class FrameSetTests : UITestFixture
    {
        private FrameSetPage _page;

        protected override void OnSetUp()
        {
            _page = Go.To<FrameSetPage>();
        }

        [Test]
        public void FrameSetPage()
        {
            _page.
                Frame1.SwitchTo().
                    Header.Should.Equal("Frame Inner 1").
                    TextInput.Set("123").
                    TextInput.Should.Equal("123").
                    SwitchToRoot<FrameSetPage>().
                Frame2.DoWithin(x => x.
                    Header.Should.Equal("Frame Inner 2").
                    Select.Should.Equal(1)).
                Frame3.SwitchTo().
                    Header.Should.Equal("Frame Inner 1").
                    TextInput.Should.BeNullOrEmpty();
        }
    }
}
