﻿using NUnit.Framework;

namespace Atata.Tests
{
    public class RandomizationTests : UITestFixture
    {
        private const int MaxTriesNumber = 100;

        private RandomizationPage _page;

        protected override void OnSetUp()
        {
            _page = Go.To<RandomizationPage>();
        }

        [Test]
        public void Randomization_Enum_Single()
        {
            var control = _page.SimpleEnum;

            control.SetRandom(out RandomizationPage.CheckBoxOptions value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out RandomizationPage.CheckBoxOptions newValue);
                control.Should.Equal(newValue);
                _page.CheckedItemsCount.Should.BeInRange(0, 1);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Enum_Multiple()
        {
            var control = _page.MultipleEnums;

            control.SetRandom(out RandomizationPage.CheckBoxOptions value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out RandomizationPage.CheckBoxOptions newValue);
                control.Should.Equal(newValue);
                _page.CheckedItemsCount.Should.BeInRange(2, 4);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Enum_MultipleWithExcluding()
        {
            var control = _page.MultipleEnumsExcludingNoneBDF;

            control.SetRandom(out RandomizationPage.CheckBoxOptions value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out RandomizationPage.CheckBoxOptions newValue);
                control.Should.Equal(newValue);
                _page.CheckedItemsCount.Should.Equal(2);
                control.Should.Not.HaveChecked(
                    RandomizationPage.CheckBoxOptions.None |
                    RandomizationPage.CheckBoxOptions.OptionB |
                    RandomizationPage.CheckBoxOptions.OptionD |
                    RandomizationPage.CheckBoxOptions.OptionF);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Enum_MultipleWithIncluding()
        {
            var control = _page.MultipleEnumsIncludingABDEF;

            control.SetRandom(out RandomizationPage.CheckBoxOptions value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out RandomizationPage.CheckBoxOptions newValue);
                control.Should.Equal(newValue);
                _page.CheckedItemsCount.Should.Equal(3);
                control.Should.Not.HaveChecked(
                    RandomizationPage.CheckBoxOptions.None |
                    RandomizationPage.CheckBoxOptions.OptionC);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Enum_Nullable()
        {
            var control = _page.EnumSelect;

            control.SetRandom(out RandomizationPage.SelectOption value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out RandomizationPage.SelectOption newValue);
                control.Should.Equal(newValue);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_String_WithIncluding()
        {
            var control = _page.TextSelect;

            control.SetRandom(out string value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out string newValue);
                control.Should.Equal(newValue);
                control.Should.Not.Equal("Option D");

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Int()
        {
            var control = _page.IntSelect;

            control.SetRandom(out int value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out int newValue);
                control.Should.Equal(newValue);
                control.Should.BeInRange(1, 2);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Int_WithIncluding()
        {
            var control = _page.IntSelectUsingInclude;

            control.SetRandom(out int value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out int newValue);
                control.Should.Equal(newValue);
                control.Should.BeLessOrEqual(3);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }

        [Test]
        public void Randomization_Bool()
        {
            var control = _page.OptionA;

            control.SetRandom(out bool value);

            for (int i = 0; i < MaxTriesNumber; i++)
            {
                control.SetRandom(out bool newValue);
                control.Should.Equal(newValue);

                if (newValue != value)
                    return;
            }

            Assert.Fail();
        }
    }
}
