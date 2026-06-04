Option Strict On
Option Explicit On

Imports System.ComponentModel

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    Private components As IContainer

    <DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Friend WithEvents DriveLabel As Label
    Friend WithEvents ActivityCaptionLabel As Label
    Friend WithEvents StatusLabel As Label
    Friend WithEvents DotIndicator As DotIndicatorControl

    <DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New Container()

        Me.DriveLabel = New Label()
        Me.ActivityCaptionLabel = New Label()
        Me.StatusLabel = New Label()
        Me.DotIndicator = New DotIndicatorControl()
        Me.SuspendLayout()

        Me.DriveLabel.AutoSize = True
        Me.DriveLabel.Location = New Point(16, 16)
        Me.DriveLabel.Name = "DriveLabel"
        Me.DriveLabel.Size = New Size(140, 15)
        Me.DriveLabel.TabIndex = 0
        Me.DriveLabel.Text = "Monitored Drive: --"

        Me.ActivityCaptionLabel.AutoSize = True
        Me.ActivityCaptionLabel.Location = New Point(16, 48)
        Me.ActivityCaptionLabel.Name = "ActivityCaptionLabel"
        Me.ActivityCaptionLabel.Size = New Size(85, 15)
        Me.ActivityCaptionLabel.TabIndex = 1
        Me.ActivityCaptionLabel.Text = "Read Activity:"

        Me.DotIndicator.BackColor = Color.Transparent
        Me.DotIndicator.IsAvailable = True
        Me.DotIndicator.IsLit = False
        Me.DotIndicator.Location = New Point(108, 42)
        Me.DotIndicator.Name = "DotIndicator"
        Me.DotIndicator.Size = New Size(28, 28)
        Me.DotIndicator.TabIndex = 2

        Me.StatusLabel.AutoSize = True
        Me.StatusLabel.Location = New Point(16, 88)
        Me.StatusLabel.Name = "StatusLabel"
        Me.StatusLabel.Size = New Size(70, 15)
        Me.StatusLabel.TabIndex = 3
        Me.StatusLabel.Text = "Status: --"

        Me.AutoScaleDimensions = New SizeF(7F, 15F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(280, 120)
        Me.Controls.Add(Me.StatusLabel)
        Me.Controls.Add(Me.DotIndicator)
        Me.Controls.Add(Me.ActivityCaptionLabel)
        Me.Controls.Add(Me.DriveLabel)
        Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = True
        Me.Name = "MainForm"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Text = "Drive Read Indicator"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
