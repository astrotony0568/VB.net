Option Strict On
Option Explicit On

Public Module Program

    <STAThread()>
    Public Sub Main()
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            Application.Run(New MainForm())
        Catch ex As Exception
            MessageBox.Show(
                "Drive Read Indicator failed to start." & Environment.NewLine & Environment.NewLine & ex.Message,
                "Drive Read Indicator",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

End Module
