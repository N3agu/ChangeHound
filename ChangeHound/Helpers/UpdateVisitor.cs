using LibreHardwareMonitor.Hardware;

namespace ChangeHound.Helpers {
    // A required class for LibreHardwareMonitor
    public class UpdateVisitor : IVisitor {
        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware) => hardware.Update();
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
