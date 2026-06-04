Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Diagnostics

Public Class DriveReadMonitor
    Implements IDisposable

    Private Const CategoryName As String = "LogicalDisk"
    Private Const ReadBytesCounterName As String = "Disk Read Bytes/sec"
    Private Const ReadOpsCounterName As String = "Disk Reads/sec"

    Private ReadOnly _rawTargetDrive As String
    Private ReadOnly _targetDrive As String
    Private ReadOnly _readThresholdBytesPerSecond As Single

    Private _readBytesCounter As PerformanceCounter
    Private _readOpsCounter As PerformanceCounter

    Private _isAvailable As Boolean = False
    Private _lastReadBytesPerSecond As Single = 0F
    Private _lastReadOpsPerSecond As Single = 0F
    Private _lastErrorMessage As String = String.Empty
    Private _disposed As Boolean = False

    Public ReadOnly Property RawTargetDrive As String
        Get
            Return _rawTargetDrive
        End Get
    End Property

    Public ReadOnly Property TargetDrive As String
        Get
            Return _targetDrive
        End Get
    End Property

    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return _isAvailable
        End Get
    End Property

    Public ReadOnly Property LastReadBytesPerSecond As Single
        Get
            Return _lastReadBytesPerSecond
        End Get
    End Property

    Public ReadOnly Property LastReadOpsPerSecond As Single
        Get
            Return _lastReadOpsPerSecond
        End Get
    End Property

    Public ReadOnly Property LastErrorMessage As String
        Get
            Return _lastErrorMessage
        End Get
    End Property

    Public Sub New(targetDrive As String, readThresholdBytesPerSecond As Single)
        If targetDrive Is Nothing Then
            Throw New ArgumentNullException(NameOf(targetDrive))
        End If
        _rawTargetDrive = targetDrive
        _targetDrive = NormalizeDriveInstance(targetDrive)
        _readThresholdBytesPerSecond = readThresholdBytesPerSecond
    End Sub

    Public Shared Function NormalizeDriveInstance(input As String) As String
        If input Is Nothing Then Return String.Empty
        Dim value As String = input.Trim().ToUpperInvariant()
        If value.Length = 0 Then Return String.Empty

        If value.EndsWith("\", StringComparison.Ordinal) Then
            value = value.TrimEnd("\"c)
        End If

        If value.Length = 1 Then
            value &= ":"
        End If

        Return value
    End Function

    Public Function TryInitialize() As Boolean
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(DriveReadMonitor))
        End If

        DisposeCounters()
        _isAvailable = False
        _lastErrorMessage = String.Empty

        If String.IsNullOrEmpty(_targetDrive) Then
            _lastErrorMessage = "Target drive is empty."
            Return False
        End If

        Try
            If Not PerformanceCounterCategory.Exists(CategoryName) Then
                _lastErrorMessage = $"Performance counter category '{CategoryName}' is not available."
                Return False
            End If

            Dim category As New PerformanceCounterCategory(CategoryName)
            Dim instances As String() = category.GetInstanceNames()
            Dim found As Boolean = instances.Any(
                Function(name) String.Equals(name, _targetDrive, StringComparison.OrdinalIgnoreCase))
            If Not found Then
                _lastErrorMessage = $"Drive {_targetDrive} not available."
                Return False
            End If

            _readBytesCounter = New PerformanceCounter(CategoryName, ReadBytesCounterName, _targetDrive, True)
            _readOpsCounter = New PerformanceCounter(CategoryName, ReadOpsCounterName, _targetDrive, True)

            Try
                _readBytesCounter.NextValue()
                _readOpsCounter.NextValue()
            Catch ex As Exception
                _lastErrorMessage = "Failed to prime counters: " & ex.Message
                DisposeCounters()
                Return False
            End Try

            _isAvailable = True
            Return True

        Catch ex As Exception
            _lastErrorMessage = "Initialization failed: " & ex.Message
            DisposeCounters()
            _isAvailable = False
            Return False
        End Try
    End Function

    Public Function Sample() As Boolean
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(DriveReadMonitor))
        End If

        If Not _isAvailable OrElse _readBytesCounter Is Nothing OrElse _readOpsCounter Is Nothing Then
            Return False
        End If

        Try
            _lastReadBytesPerSecond = _readBytesCounter.NextValue()
            _lastReadOpsPerSecond = _readOpsCounter.NextValue()
        Catch ex As InvalidOperationException
            ' Instance disappeared (USB unplug, drive offline, etc.).
            _lastErrorMessage = "Counter instance lost: " & ex.Message
            DisposeCounters()
            _isAvailable = False
            Return False
        Catch ex As Win32Exception
            _lastErrorMessage = "Counter system error: " & ex.Message
            DisposeCounters()
            _isAvailable = False
            Return False
        Catch ex As Exception
            _lastErrorMessage = "Counter read failed: " & ex.Message
            DisposeCounters()
            _isAvailable = False
            Return False
        End Try

        Return _lastReadBytesPerSecond >= _readThresholdBytesPerSecond OrElse _lastReadOpsPerSecond > 0F
    End Function

    Private Sub DisposeCounters()
        Try
            If _readBytesCounter IsNot Nothing Then
                _readBytesCounter.Close()
                _readBytesCounter.Dispose()
                _readBytesCounter = Nothing
            End If
        Catch
        End Try

        Try
            If _readOpsCounter IsNot Nothing Then
                _readOpsCounter.Close()
                _readOpsCounter.Dispose()
                _readOpsCounter = Nothing
            End If
        Catch
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _isAvailable = False
        DisposeCounters()
        GC.SuppressFinalize(Me)
    End Sub

End Class
