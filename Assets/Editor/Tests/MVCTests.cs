using NUnit.Framework;

namespace Editor.Tests
{
    public class NewTestScript {

        [Test]
        public void TestMVC() {
            var model = new ItemModel();
            var view = new ItemEntryViewMock();
            var controller = new ItemEntryController(view, model);
            controller.Setup();

            var button = (MockButtonView) view.ButtonView;
            button.SimulateButtonPress();

            Assert.AreEqual(1, model.ItemCount);
        }

    }
}
