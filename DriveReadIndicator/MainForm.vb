Option Strict On
Option Explicit On

Public Class MainForm

    ' ============================================================================
    '  CHANGE THIS to the drive you want to monitor (e.g. "C:", "D:", "E:").
    '  Accepts "E", "E:" or "E:\" — all are normalised to the "E:" form.
    '  This is the single line you edit to pick the monitored drive.
    ' ============================================================================
    Private Const TargetDriveInstance As String = "C:"
    Private Const MonitorIntervalMs As Integer = 200
    Private Const BlinkIntervalMs As Integer = 150
    Private Const ActivityHoldMs As Integer = 500
    Private Const ReadThresholdBytesPerSecond As Single = 1024.0F

    Private Const RecoveryRetryMs As Integer = 3000

    Private _monitor As DriveReadMonitor
    Private _lastReadActivityUtc As DateTime = DateTime.MinValue
    Private _blinkState As Boolean = False
    Private _msSinceLastRecoveryAttempt As Integer = 0

    Private WithEvents _monitorTimer As Timer
    Private WithEvents _blinkTimer As Timer

    Public Sub New()
        InitializeComponent()
        DriveLabel.Text = "Monitored Drive: " & DriveReadMonitor.NormalizeDriveInstance(TargetDriveInstance)
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        _monitor = New DriveReadMonitor(TargetDriveInstance, ReadThresholdBytesPerSecond)
        TryInitializeMonitor()

        _monitorTimer = New Timer() With {.Interval = MonitorIntervalMs}
        _blinkTimer = New Timer() With {.Interval = BlinkIntervalMs}
        _monitorTimer.Start()
        _blinkTimer.Start()

        ApplyVisualForCurrentState()
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If _monitorTimer IsNot Nothing Then
                _monitorTimer.Stop()
                _monitorTimer.Dispose()
                _monitorTimer = Nothing
            End If
            If _blinkTimer IsNot Nothing Then
                _blinkTimer.Stop()
                _blinkTimer.Dispose()
                _blinkTimer = Nothing
            End If
            If _monitor IsNot Nothing Then
                _monitor.Dispose()
                _monitor = Nothing
            End If
        Catch ex As Exception
            Debug.WriteLine("Cleanup error: " & ex.Message)
        End Try
    End Sub

    Private Sub TryInitializeMonitor()
        If _monitor Is Nothing Then Return

        Dim ok As Boolean = False
        Try
            ok = _monitor.TryInitialize()
        Catch ex As Exception
            Debug.WriteLine("Monitor init exception: " & ex.Message)
        End Try

        If ok Then
            SetStatus("Monitoring " & _monitor.TargetDrive)
            DotIndicator.IsAvailable = True
        Else
            Dim reason As String = If(String.IsNullOrEmpty(_monitor.LastErrorMessage),
                                      "Drive " & _monitor.TargetDrive & " not available.",
                                      _monitor.LastErrorMessage)
            SetStatus(reason)
            DotIndicator.IsAvailable = False
            DotIndicator.IsLit = False
        End If
    End Sub

    ' ===== Timers =====

    Private Sub MonitorTimer_Tick(sender As Object, e As EventArgs) Handles _monitorTimer.Tick
        If _monitor Is Nothing Then Return

        If _monitor.IsAvailable Then
            Dim active As Boolean
            Try
                active = _monitor.Sample()
            Catch ex As Exception
                Debug.WriteLine("Sample exception: " & ex.Message)
                active = False
            End Try

            If active Then
                _lastReadActivityUtc = DateTime.UtcNow
            End If

            If Not _monitor.IsAvailable Then
                HandleMonitorBecameUnavailable()
            End If
        Else
            _msSinceLastRecoveryAttempt += MonitorIntervalMs
            If _msSinceLastRecoveryAttempt >= RecoveryRetryMs Then
                _msSinceLastRecoveryAttempt = 0
                TryInitializeMonitor()
            End If
        End If
    End Sub

    Private Sub BlinkTimer_Tick(sender As Object, e As EventArgs) Handles _blinkTimer.Tick
        If _monitor Is Nothing OrElse Not _monitor.IsAvailable Then
            DotIndicator.IsLit = False
            Exit Sub
        End If

        Dim sinceLastMs As Double = (DateTime.UtcNow - _lastReadActivityUtc).TotalMilliseconds
        If sinceLastMs <= ActivityHoldMs Then
            _blinkState = Not _blinkState
            DotIndicator.IsLit = _blinkState
        Else
            _blinkState = False
            DotIndicator.IsLit = False
        End If
    End Sub

    Private Sub HandleMonitorBecameUnavailable()
        DotIndicator.IsAvailable = False
        DotIndicator.IsLit = False
        _msSinceLastRecoveryAttempt = 0
        Dim reason As String = If(String.IsNullOrEmpty(_monitor.LastErrorMessage),
                                  "Drive " & _monitor.TargetDrive & " not available.",
                                  _monitor.LastErrorMessage)
        SetStatus(reason)
    End Sub

    Private Sub ApplyVisualForCurrentState()
        If _monitor Is Nothing OrElse Not _monitor.IsAvailable Then
            DotIndicator.IsAvailable = False
            DotIndicator.IsLit = False
        Else
            DotIndicator.IsAvailable = True
        End If
    End Sub

    Private Sub SetStatus(text As String)
        StatusLabel.Text = "Status: " & text
    End Sub

End Class
