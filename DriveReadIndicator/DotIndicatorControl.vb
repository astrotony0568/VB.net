Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class DotIndicatorControl
    Inherits Control

    Private _isLit As Boolean = False
    Private _isAvailable As Boolean = True

    Private _activeColor As Color = Color.FromArgb(255, 64, 64)      
    Private _inactiveColor As Color = Color.FromArgb(70, 70, 70)     
    Private _unavailableColor As Color = Color.FromArgb(40, 40, 40)  
    Private _borderColor As Color = Color.FromArgb(20, 20, 20)

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.UserPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Size = New Size(28, 28)
        TabStop = False
    End Sub

    <Category("Appearance"), DefaultValue(False)>
    Public Property IsLit As Boolean
        Get
            Return _isLit
        End Get
        Set(value As Boolean)
            If _isLit <> value Then
                _isLit = value
                Invalidate()
            End If
        End Set
    End Property

    <Category("Appearance"), DefaultValue(True)>
    Public Property IsAvailable As Boolean
        Get
            Return _isAvailable
        End Get
        Set(value As Boolean)
            If _isAvailable <> value Then
                _isAvailable = value
                Invalidate()
            End If
        End Set
    End Property

    <Category("Appearance")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property ActiveColor As Color
        Get
            Return _activeColor
        End Get
        Set(value As Color)
            _activeColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeActiveColor() As Boolean
        Return _activeColor <> Color.FromArgb(255, 64, 64)
    End Function

    Public Sub ResetActiveColor()
        ActiveColor = Color.FromArgb(255, 64, 64)
    End Sub

    <Category("Appearance")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property InactiveColor As Color
        Get
            Return _inactiveColor
        End Get
        Set(value As Color)
            _inactiveColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeInactiveColor() As Boolean
        Return _inactiveColor <> Color.FromArgb(70, 70, 70)
    End Function

    Public Sub ResetInactiveColor()
        InactiveColor = Color.FromArgb(70, 70, 70)
    End Sub

    <Category("Appearance")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property UnavailableColor As Color
        Get
            Return _unavailableColor
        End Get
        Set(value As Color)
            _unavailableColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeUnavailableColor() As Boolean
        Return _unavailableColor <> Color.FromArgb(40, 40, 40)
    End Function

    Public Sub ResetUnavailableColor()
        UnavailableColor = Color.FromArgb(40, 40, 40)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim diameter As Integer = Math.Max(4, Math.Min(Width, Height) - 2)
        Dim x As Single = CSng((Width - diameter) / 2.0)
        Dim y As Single = CSng((Height - diameter) / 2.0)
        Dim rect = New RectangleF(x, y, diameter, diameter)

        Dim fillColor As Color
        If Not _isAvailable Then
            fillColor = _unavailableColor
        ElseIf _isLit Then
            fillColor = _activeColor
        Else
            fillColor = _inactiveColor
        End If

        Using brush As New SolidBrush(fillColor)
            g.FillEllipse(brush, rect)
        End Using

        Using pen As New Pen(_borderColor, 1.0F)
            g.DrawEllipse(pen, rect)
        End Using

        If _isAvailable AndAlso _isLit Then
            Dim hiRect = New RectangleF(
                rect.X + rect.Width * 0.18F,
                rect.Y + rect.Height * 0.15F,
                rect.Width * 0.35F,
                rect.Height * 0.25F)
            Using hi As New SolidBrush(Color.FromArgb(110, 255, 255, 255))
                g.FillEllipse(hi, hiRect)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnPaintBackground(pevent As PaintEventArgs)
    End Sub

End Class
