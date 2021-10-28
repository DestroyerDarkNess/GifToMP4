Public Class Form1

#Region " Declare "

    Private PathExecutable As String = IO.Path.Combine(IO.Path.GetTempPath, "ffmpeg.exe")

    Private FfmpegArgument As String = <a><![CDATA[ -i "%FileGif%" -movflags faststart -pix_fmt yuv420p -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" "%FileMP4%" ]]></a>.Value


#End Region

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        Convert(OpenFileDialog1.FileName)
    End Sub

#Region " Dragger "

    Private Sub Label1_DragDrop(sender As Object, e As DragEventArgs) Handles Label1.DragDrop
        Dim FilePath As String() = e.Data.GetData(DataFormats.FileDrop)
        If IO.Path.GetExtension(FilePath.ToList.FirstOrDefault).ToUpper = ".GIF" Then
            Convert(FilePath.ToList.FirstOrDefault)
        End If
    End Sub

    Private Sub Panel1_DragDrop(sender As Object, e As DragEventArgs) Handles Panel1.DragDrop
        Dim FilePath As String() = e.Data.GetData(DataFormats.FileDrop)
        If IO.Path.GetExtension(FilePath.ToList.FirstOrDefault).ToUpper = ".GIF" Then
            Convert(FilePath.ToList.FirstOrDefault)
        End If
    End Sub

    Private Sub Label1_DragEnter(sender As Object, e As DragEventArgs) Handles Label1.DragEnter
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub Panel1_DragEnter(sender As Object, e As DragEventArgs) Handles Panel1.DragEnter
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

#End Region

#Region " Methods "

    Public Sub Convert(ByVal GifPath As String)
        If IO.File.Exists(GifPath) = True Then

            If SaveFileDialog1.ShowDialog = DialogResult.OK Then

                Me.ProgressBar1.Value = 10

                If IO.File.Exists(SaveFileDialog1.FileName) = True Then
                    IO.File.Delete(SaveFileDialog1.FileName)
                End If

                Me.ProgressBar1.Value = 30

                If CheckFfmpeg() = False Then
                    If ExtractFFmpeg() = False Then
                        Me.ProgressBar1.Visible = False
                        Me.Height = 180
                        Exit Sub
                    End If
                End If

                Me.Height = 57
                ProgressBar1.Visible = True

                Dim ArgumentsStr As String = FfmpegArgument.Replace("%FileGif%", GifPath).Replace("%FileMP4%", SaveFileDialog1.FileName)

                Dim Proc As New System.Diagnostics.Process

                Me.ProgressBar1.Value = 50

                Proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                Proc.StartInfo.CreateNoWindow = False
                Proc.StartInfo.UseShellExecute = True
                Proc.StartInfo.FileName = PathExecutable
                Proc.StartInfo.Arguments = ArgumentsStr
                Proc.Start()

                Me.ProgressBar1.Value = 90

                StartMonitor(Proc)

            End If

        End If
    End Sub

    Private Function CheckFfmpeg() As Boolean
        Return IO.File.Exists(PathExecutable)
    End Function

    Public Function ExtractFFmpeg() As Boolean
        Try
            IO.File.WriteAllBytes(PathExecutable, My.Resources.ffmpeg)
            Return IO.File.Exists(PathExecutable)
        Catch ex As Exception
            Return False
        End Try
    End Function

#End Region

#Region " Process Monitor "

    Public Sub StartMonitor(ByVal TargetProcess As Process)
        Dim Asynctask As New Task(New Action(Sub()

                                                 For i As Integer = 0 To 2
                                                     Try

                                                         If CheckProcess(TargetProcess.Id) = False Then
                                                             Me.BeginInvoke(Sub()
                                                                                Me.ProgressBar1.Visible = False
                                                                                Me.Height = 180
                                                                                Me.ProgressBar1.Value = 0
                                                                            End Sub)
                                                             Exit Sub
                                                         End If

                                                     Catch ex As Exception
                                                         Me.BeginInvoke(Sub()
                                                                            Me.ProgressBar1.Visible = False
                                                                            Me.Height = 180
                                                                            Me.ProgressBar1.Value = 0
                                                                        End Sub)
                                                         Exit Sub
                                                     End Try
                                                     System.Threading.Thread.Sleep(1000)
                                                     i -= 1
                                                 Next

                                             End Sub), TaskCreationOptions.PreferFairness)
        Asynctask.Start()
    End Sub

    Private Function CheckProcess(ByVal ID As Integer) As Boolean
        Try
            Dim Proc As Process = Process.GetProcessById(ID)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

#End Region

End Class
