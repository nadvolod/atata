﻿namespace Atata.IntegrationTests.Controls;

public class ControlListTests : UITestFixture
{
    [Test]
    public void OfTextInput()
    {
        var sut = Go.To<BasicControlsPage>().AllTextInputs;

        sut[1].SetRandom(out string lastName);

        sut[0].Should.Not.Equal(lastName);
        sut[1].Should.Equal(lastName);
        sut[2].Should.Not.Equal(lastName);
    }

    [Test]
    public void OfCheckBox()
    {
        var sut = Go.To<CheckBoxListPage>().AllItems;
        int actualCount = 12;

        sut.Should.HaveCount(actualCount);
        sut.Should.Not.HaveCount(1);
        sut.Count.Should.Equal(actualCount);
        sut.Count.Should.BeGreaterOrEqual(actualCount);
        sut.Count.Should.Not.BeLess(actualCount);
        sut.Should.Not.Contain(true);
        sut.Should.Not.Contain(x => x.IsChecked);
        sut[x => x.IsChecked].Should.Not.Exist();
        sut[2].Check();
        sut.Should.Contain(x => x.IsChecked);
        sut[2].IsChecked.Should.BeTrue();
    }

    [Test]
    public void OfRadioButton()
    {
        var sut = Go.To<RadioButtonListPage>().IntegerItemsControl.Items;

        sut.Should.HaveCount(4);
        sut.Count.Should.Equal(4);
        sut.Should.Not.Contain(true);
        sut.Should.Not.Contain(x => x.IsChecked);
        sut[x => x.IsChecked].Should.Not.Exist();
        sut[3].Check();
        sut.Should.Contain(x => x.IsChecked);
        sut[3].IsChecked.Should.BeTrue();
        sut[1].Check();
        sut[3].IsChecked.Should.BeFalse();
        sut[1].Should.BeChecked();
    }

    [Test]
    public void WithDeclaredAttributes() =>
        Go.To<ListPage>()
            .ProductNameTextContolList[0].Should.Equal("Phone")
            .ProductNameTextContolList[1].Should.Equal("Book")
            .ProductNameTextContolList.Count.Should.Equal(3)
            .ProductNameTextContolList.Should.EqualSequence("Phone", "Book", "Table")
            .ProductPercentNumberContolList.Should.EqualSequence(0.05m, 0.10m, 0.15m)
            .ProductPercentNumberContolList[1].Should.Equal(0.10m);

    [Test]
    public void OfDescendantsAsControls()
    {
        var sut = Go.To<ListPage>().ItemsControlOfDescendantsAsControls.Items;

        sut.Count.Should.Equal(6);

        var tagNames = sut.Select(x => x.Scope.TagName).ToArray();

        tagNames.Where(x => x == "li").Should().HaveCount(3);
        tagNames.Where(x => x == "span").Should().HaveCount(3);
    }

    [Test]
    public void OfChildrenAsControls()
    {
        var sut = Go.To<ListPage>().ItemsControlOfChildrenAsControls.Items;

        sut.Count.Should.Equal(3);

        foreach (var item in sut)
            item.Scope.TagName.Should().Be("li");
    }

    [Test]
    public void SelectContentsByExtraXPath()
    {
        var sut = Go.To<TablePage>().CarsTable.Rows;

        sut.SelectContentsByExtraXPath("td[4]").Should.Contain("Saloon");
        sut.SelectContentsByExtraXPath("/td[4]", "Categories").Should.HaveCount(18);
        sut.SelectContentsByExtraXPath<int>("/td[3]", "Years").Should.Contain(2001, 2007);
    }

    [Test]
    public void SelectData_JSPath()
    {
        var sut = Go.To<TablePage>().CarsTable.Rows;

        sut.SelectData<string>("querySelector('td').getAttribute('data-id')").Should.Contain("2");
        sut.SelectData<int?>("querySelector('td').getAttribute('data-id')").Should.Contain(1, 2, null);
    }

    [Test]
    public void SelectDataByExtraXPath()
    {
        var sut = Go.To<TablePage>().CarsTable.Rows;

        sut.SelectDataByExtraXPath<string>("td[1]", "getAttribute('data-id')").Should.Contain("2");
        sut.SelectDataByExtraXPath<int?>("/td[1]", "getAttribute('data-id')").Should.Contain(1, 2, null);
        sut.SelectDataByExtraXPath<int?>("/td[1][@data-id]", "getAttribute('data-id')").Should.EqualSequence(1, 2, 3, 4);
    }

    [Test]
    public void GetByXPathCondition()
    {
        var sut = Go.To<TablePage>().CarsTable.Rows;

        sut.GetByXPathCondition("Nissan", "td[1][text()='Nissan']").CarMake.Should.Equal("Nissan");
    }

    [Test]
    public void GetAllByXPathCondition()
    {
        var sut = Go.To<TablePage>().CarsTable.Rows;

        sut.GetAllByXPathCondition("Nissan", "td[1][text()='Nissan']").Should.HaveCount(2);
    }

    [Test]
    public void WhenEmpty()
    {
        var sut = Go.To<TablePage>().EmptyTable.Rows;

        using (StopwatchAsserter.WithinSeconds(0))
            sut.Should.BeEmpty();

        using (StopwatchAsserter.WithinSeconds(0))
            sut.Count.Should.Equal(0);

        using (StopwatchAsserter.WithinSeconds(0))
            sut.Should.HaveCount(0);

        using (StopwatchAsserter.WithinSeconds(0))
            sut.AsEnumerable().Should().BeEmpty();
    }

    [Test]
    public void PredicateIndexer_UsingExternalIndexer()
    {
        var sut = Go.To<TablePage>().NumberedTable.Rows;

        var dictionaries = new[]
        {
            new Dictionary<string, string>
            {
                ["Name"] = "Item 1",
                ["Number"] = "1"
            },
            new Dictionary<string, string>
            {
                ["Name"] = "Item 2",
                ["Number"] = "2"
            }
        };

        foreach (var dictionary in dictionaries)
            sut[x => x.Name.Value.Contains(dictionary["Name"])]
                .Name.Should.Equal(dictionary["Name"]);
    }

    public class UsesScopeCache : UITestFixture
    {
        private ControlList<TablePage.NumberedTableRow, TablePage> _sut;

        protected override void OnSetUp()
        {
            var table = Go.To<TablePage>().NumberedTable;
            table.Metadata.Push(new UsesScopeCacheAttribute());

            _sut = table.Rows;
            _sut.Metadata.Push(new UsesScopeCacheAttribute { TargetSelfAndChildren = true });
        }

        [Test]
        public void ReuseItem()
        {
            var item = _sut[x => x.Name == "Item 2"];
            item.Number.Should.Be(2);
            item.Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehavior();
        }

        [Test]
        public void SameItem_BySamePredicate()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            var entries = GetLastLogEntries(10);
            entries[0].SectionStart.Should().BeOfType<VerificationLogSection>();
            entries[1].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[3].SectionEnd.Should().Be(entries[2].SectionStart);
            entries[4].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[5].SectionEnd.Should().Be(entries[4].SectionStart);
            entries[6].SectionStart.Should().BeOfType<ElementFindLogSection>();
            entries[7].SectionEnd.Should().Be(entries[6].SectionStart);
            entries[8].SectionEnd.Should().Be(entries[1].SectionStart);
            entries[9].SectionEnd.Should().Be(entries[0].SectionStart);
        }

        [Test]
        public void SameItem_BySameIndex()
        {
            _sut[1].Number.Should.Be(2);
            _sut[1].Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehavior();
        }

        [Test]
        public void SameItem_BySameXPath()
        {
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehavior();
        }

        [Test]
        public void SameItem_ByDifferentPredicate()
        {
            _sut[x => x.Number == 2 && x.Name == "Item 2"].Should.BePresent();
            _sut[x => x.Number == 2].Should.BePresent();

            var entries = GetLastLogEntries(6);
            entries[0].SectionStart.Should().BeOfType<VerificationLogSection>();
            entries[1].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().Be(entries[1].SectionStart);
            entries[3].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[4].SectionEnd.Should().Be(entries[3].SectionStart);
            entries[5].SectionEnd.Should().Be(entries[0].SectionStart);
        }

        [Test]
        public void PreviousItem_BySimilarPredicate()
        {
            _sut[x => x.Name == "Item 3"].Should.BePresent();
            _sut[x => x.Name == "Item 2"].Should.BePresent();

            var entries = GetLastLogEntries(6);
            entries[0].SectionStart.Should().BeOfType<VerificationLogSection>();
            entries[1].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().Be(entries[1].SectionStart);
            entries[3].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[4].SectionEnd.Should().Be(entries[3].SectionStart);
            entries[5].SectionEnd.Should().Be(entries[0].SectionStart);
        }

        [Test]
        public void GetCount_2Times()
        {
            _sut.Count.Should.Be(3);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void GetCount_AfterGettingItem()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void AfterClearCache()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.ClearCache();
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }

        [Test]
        public void AfterClearCache_OfPageObject()
        {
            _sut[1].Number.Should.Be(2);
            _sut.Component.Owner.ClearCache();
            _sut[1].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }
    }

    public class UsesValueCache : UITestFixture
    {
        private ControlList<TablePage.NumberedTableRow, TablePage> _sut;

        protected override void OnSetUp()
        {
            var table = Go.To<TablePage>().NumberedTable;

            _sut = table.Rows;
            _sut.Metadata.Push(new UsesValueCacheAttribute { TargetChildren = true });
        }

        [Test]
        public void ReuseItem()
        {
            var item = _sut[x => x.Name == "Item 2"];
            item.Number.Should.Be(2);
            item.Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void SameItem_BySamePredicate()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehaviorAnd3ElementFindSections();
        }

        [Test]
        public void SameItem_BySameIndex()
        {
            _sut[1].Number.Should.Be(2);
            _sut[1].Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehaviorAnd3ElementFindSections();
        }

        [Test]
        public void SameItem_BySameXPath()
        {
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationWithExecuteBehaviorAnd3ElementFindSections();
        }

        [Test]
        public void SameItem_ByDifferentPredicate()
        {
            _sut[x => x.Number == 2 && x.Name == "Item 2"].Should.BePresent();
            _sut[x => x.Number == 2].Should.BePresent();

            AssertThatLastLogSectionIsVerificationWith2ElementFindSections();
        }

        [Test]
        public void PreviousItem_BySimilarPredicate()
        {
            _sut[x => x.Name == "Item 3"].Should.BePresent();
            _sut[x => x.Name == "Item 2"].Should.BePresent();

            AssertThatLastLogSectionIsVerificationWith2ElementFindSections();
        }

        [Test]
        public void GetCount_2Times()
        {
            _sut.Count.Should.Be(3);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationWith2ElementFindSections();
        }

        [Test]
        public void GetCount_AfterGettingItem()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationWith2ElementFindSections();
        }

        [Test]
        public void AfterClearCache()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.ClearCache();
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }

        [Test]
        public void AfterClearCache_OfPageObject()
        {
            _sut[1].Number.Should.Be(2);
            _sut.Component.Owner.ClearCache();
            _sut[1].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }
    }

    public class UsesCache : UITestFixture
    {
        private ControlList<TablePage.NumberedTableRow, TablePage> _sut;

        protected override void OnSetUp()
        {
            var table = Go.To<TablePage>().NumberedTable;
            table.Metadata.Push(new UsesCacheAttribute());

            _sut = table.Rows;
            _sut.Metadata.Push(new UsesCacheAttribute { TargetSelfAndChildren = true });
        }

        [Test]
        public void ReuseItem()
        {
            var item = _sut[x => x.Name == "Item 2"];
            item.Number.Should.Be(2);
            item.Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void SameItem_BySamePredicate()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            var entries = GetLastLogEntries(6);
            entries[0].SectionStart.Should().BeOfType<VerificationLogSection>();
            entries[1].SectionStart.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionStart.Should().BeOfType<ElementFindLogSection>();
            entries[3].SectionEnd.Should().Be(entries[2].SectionStart);
            entries[4].SectionEnd.Should().Be(entries[1].SectionStart);
            entries[5].SectionEnd.Should().Be(entries[0].SectionStart);
        }

        [Test]
        public void SameItem_BySameIndex()
        {
            _sut[1].Number.Should.Be(2);
            _sut[1].Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void SameItem_BySameXPath()
        {
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);
            _sut.GetByXPathCondition("td[1][.='Item 2']").Number.Should.Be(2);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void SameItem_ByDifferentPredicate()
        {
            _sut[x => x.Number == 2 && x.Name == "Item 2"].Should.BePresent();
            _sut[x => x.Number == 2].Should.BePresent();

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void PreviousItem_BySimilarPredicate()
        {
            _sut[x => x.Name == "Item 3"].Should.BePresent();
            _sut[x => x.Name == "Item 2"].Should.BePresent();

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void GetCount_2Times()
        {
            _sut.Count.Should.Be(3);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void GetCount_AfterGettingItem()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.Count.Should.Be(3);

            AssertThatLastLogSectionIsVerificationAndEmpty();
        }

        [Test]
        public void AfterClearCache()
        {
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);
            _sut.ClearCache();
            _sut[x => x.Name == "Item 2"].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }

        [Test]
        public void AfterClearCache_OfPageObject()
        {
            _sut[1].Number.Should.Be(2);
            _sut.Component.Owner.ClearCache();
            _sut[1].Number.Should.Be(2);

            var entries = GetLastLogEntries(3);
            entries[0].SectionEnd.Should().BeOfType<ElementFindLogSection>();
            entries[1].SectionEnd.Should().BeOfType<ExecuteBehaviorLogSection>();
            entries[2].SectionEnd.Should().BeOfType<VerificationLogSection>();
        }
    }
}
