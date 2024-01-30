using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace MyNotes.Classes
{
	/*
		Here's how you can get your Client ID and Client Secret for your Google Drive API integration:

		1. Access Google Cloud Platform:
			Go to the Google Cloud Platform console: https://console.cloud.google.com/
		
		2. Create or Select a Project:
			If you haven't already, create a new project by clicking on "Create Project" and giving it a relevant name.
			If you have existing projects, choose the one you want to use for your application.
		
		3. Enable Drive API:
			Click on "APIs & Services" from the side menu.
			Search for "Drive API" and click on it.
			Click on "Enable" to activate the API for your project.
		
		4. Create OAuth Credentials:
			Click on "Credentials" from the side menu.
			Click on "Create Credentials" and choose "OAuth client ID".
			Select "Desktop application" as the application type.
			Provide your application's name and other relevant details.
			Click on "Create" to generate your credentials.
		
		5. Download or Copy your Client ID and Secret:
			You'll be presented with your Client ID and Client Secret on the screen.
			Download the JSON file containing both credentials for storing securely.
			You need to specify the path to this JSON file at the 'ClientSecrets' property of this class.
			Place this JSON at the 'bin' folder, next to the .exe, name it: 'client_secrets.json'

		2. Install Required NuGet Packages:
			Use NuGet Package Manager to install:
			Google.Apis.Drive.v3
			Google.Apis.Auth
			Google.Apis.Oauth2.v2

	Check Documentation for Google Drive API v3:   https://developers.google.com/drive/api/reference/rest/v3
	 */
	public class GDriveNET : IDisposable
	{
		#region Private Members

		/// <summary>A folder to temporay store the Client's Secrets so it doesnt have to re-login every time.</summary>
		public string LOCAL_STORAGE = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");

		private Google.Apis.Drive.v3.DriveService DService = null;
		private UserCredential credentials = null;
		private FilesResource.GetRequest FilesRequest = null;

		#endregion

		#region Public Properties

		public event EventHandler OnProgressChange = delegate { };

		/// <summary>The Name of this Application</summary>
		public string APP_NAME { get; set; } = "MyNotes";

		/// <summary>the email of the end user, most be a Google Account.</summary>
		public string UserAccount { get; set; } = "user@gmail.com";

		/// <summary>Full path to the JSON file containing the Secrets to Googe Drive API.</summary>
		public string ClientSecrets { get; set; } = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");

		/// <summary>To search for a specific set of files or folders, use the query string.
		/// <para>https://developers.google.com/drive/api/guides/search-files</para>
		/// <para>name contains 'document' :Filter by file name</para>
		/// <para>fullText contains 'hello' :Files that contain the word "hello"</para>
		/// <para>mimeType='application/pdf' :List only PDF files</para>
		/// <para>mimeType = 'application/vnd.google-apps.folder' :return only folders</para>
		/// <para>mimeType != 'application/vnd.google-apps.folder' :Files that are not folders</para>
		/// </summary>
		public string FileFilters { get; set; } = "mimeType != 'application/vnd.google-apps.folder' AND name contains '{0}'";// "mimeType = 'application/vnd.google-apps.folder' OR name contains '{0}'";
		public string FolderFilter { get; set; } = "mimeType = 'application/vnd.google-apps.folder' AND name contains '{0}'";

		/// <summary>List of found Files if the 'FindFiles' method was invoked.</summary>
		public List<File> Files { get; set; }

		public File FileData { get; set; }
		public string DownloadedFilePath { get; set; }

		#endregion

		#region Contructor

		/// <summary>Google Drive Integration with .NET applications.
		/// by Jhollman Chacon - 2023</summary>
		public GDriveNET()
		{
			if (!System.IO.File.Exists(ClientSecrets))
			{
				throw new ArgumentNullException("API Credentials not found!");
			}
		}
		public void Dispose()
		{
			if (DService != null) DService.Dispose();
			credentials = null;
			FilesRequest = null;
			Files = null;
		}

		#endregion

		#region Authenticate

		/// <summary>This will trigger a browser window for the user to sign in on Google Services and grant permissions to access GDrive. 
		/// <para>Once authorized, the credentials will be stored in the specified file store for future use to persist authorization tokens and refresh them when necessary.</para>
		/// <para>This helps avoid requiring the user to sign in every time they use the application.</para></summary>
		/// <returns>'true' if authentication was success.</returns>
		public async Task<bool> Authenticate()
		{
			bool _ret = false;
			await Task.Run(() =>
			{
				_ret = Authenticate_Sync();
			});
			return _ret;
		}

		/// <summary>This will trigger a browser window for the user to sign in on Google Services and grant permissions to access GDrive. 
		/// <para>Once authorized, the credentials will be stored in the specified file store for future use to persist authorization tokens and refresh them when necessary.</para>
		/// <para>This helps avoid requiring the user to sign in every time they use the application.</para></summary>
		/// <param name="pUserAccount"></param>
		/// <returns>'true' if authentication was success.</returns>
		public async Task<bool> Authenticate(string pUserAccount)
		{
			bool _ret = false;
			await Task.Run(() =>
			{
				UserAccount = pUserAccount;
				_ret = Authenticate_Sync();
			});
			return _ret;
		}
		private bool Authenticate_Sync()
		{
			bool _ret = false;
			try
			{
				using (var stream = new System.IO.FileStream(ClientSecrets, System.IO.FileMode.Open, System.IO.FileAccess.Read))
				{
					this.credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
										GoogleClientSecrets.FromStream(stream).Secrets,
										new[] { DriveService.Scope.Drive },
										UserAccount,
										CancellationToken.None,
										new FileDataStore(LOCAL_STORAGE)).Result;

					// Check for authorization errors
					if (this.credentials.Token.IsStale)
					{
						// Attempt token refresh
						credentials.RefreshTokenAsync(CancellationToken.None);
					}

					// Create Drive service using the obtained credentials
					DService = new DriveService(new BaseClientService.Initializer()
					{
						HttpClientInitializer = this.credentials,
						ApplicationName = APP_NAME,
					});
					_ret = true;
				}
			}
			catch (Google.GoogleApiException ex)
			{
				if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
				{
					try
					{
						// Retrieve credential from service
						credentials = DService.HttpClientInitializer as UserCredential;

						// Refresh token
						credentials.RefreshTokenAsync(CancellationToken.None);

						// Update DriveService (optional)
						DService = new DriveService(new BaseClientService.Initializer()
						{
							HttpClientInitializer = credentials,
							ApplicationName = APP_NAME,
						});

						// Retry the request with the refreshed token
					}
					catch (Exception)
					{
						// Initiate re-authorization if refresh fails
					}
				}
				else
				{
					// Handle other API errors
				}
			}
			catch (Exception)
			{
				throw;
			}
			return _ret;
		}

		#endregion

		#region Get/Find Files

		/// <summary>Select a GDrive file or Folder by it's ID.</summary>
		/// <param name="pFileID">File ID</param>
		/// <returns>'true' if the file was found and selected. 
		/// <para>use 'FilesRequest' and 'FileData' to interact with the file.</para></returns>
		public File GetFile(string pFileID, bool IncludeParents = true)
		{
			Google.Apis.Drive.v3.Data.File _ret = null;
			try
			{
				FilesRequest = DService.Files.Get(pFileID);
				if (FilesRequest != null)
				{
					FilesRequest.Fields = "id, name, mimeType, createdTime, modifiedTime, size, parents";
					FileData = FilesRequest.Execute();

					if (FileData.Parents != null && IncludeParents)
					{
						foreach (var parentID in FileData.Parents)
						{
							var parentFile = DService.Files.Get(parentID);
							if (parentFile != null)
							{
								var parentData = parentFile.Execute();
								FileData.Name = System.IO.Path.Combine(parentData.Name, FileData.Name);
							}
						}
					}
					_ret = FileData;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		/// <summary>List files in the user's Drive.
		/// <para>Using the 'FileFilters' property.</para></summary>
		public List<File> FindFiles() => FindFiles_Sync(this.FileFilters);

		/// <summary>List files in the user's Drive.
		/// <para>Using a custom Filter. Incorrect filters will throw a 'Bad Request' error.</para></summary>
		/// <param name="pFilter">Custom Filter, check https://developers.google.com/drive/api/guides/search-files for details.</param>
		/// <returns>a List with all found Files</returns>
		public async Task<List<File>> FindFiles(string pFilter)
		{
			List<File> _ret = null;
			await Task.Run(() =>
			{
				_ret = FindFiles_Sync(pFilter);
			});
			return _ret;
		}

		private List<File> FindFiles_Sync(string pFilter)
		{
			List<Google.Apis.Drive.v3.Data.File> _ret = null;
			try
			{
				if (DService != null)
				{
					//Google Drive API v3 Search query terms and operators -> https://developers.google.com/drive/api/guides/ref-search-terms

					FilesResource.ListRequest FilesRequest = DService.Files.List();
					FilesRequest.Spaces = "drive";
					FilesRequest.Fields = "nextPageToken, files(id, name, mimeType, createdTime, modifiedTime, size, parents)";
					FilesRequest.PageSize = 100;    //Pagination: If there are more files than the specified page size,
													//use the nextPageToken property in the response to fetch subsequent pages			

					if (!string.IsNullOrEmpty(pFilter))
					{
						pFilter = pFilter.Replace("*", string.Empty);
						FilesRequest.Q = string.Format(this.FileFilters, pFilter);
					}

					if (FilesRequest != null)
					{
						// Retrieve file metadata
						this.Files = FilesRequest.Execute().Files.ToList();
						int Counter = 1;
						foreach (var file in Files)
						{
							if (file.Parents != null)
							{
								foreach (var parentID in file.Parents)
								{
									var parentFile = DService.Files.Get(parentID);
									if (parentFile != null)
									{
										var parentData = parentFile.Execute();
										file.Name = System.IO.Path.Combine(parentData.Name, file.Name);
									}
								}
							}
							Console.WriteLine($"{Counter} - File Name: {file.Name}, File Type: {file.MimeType}, File ID: {file.Id}");
							Counter++;
						}
						_ret = Files;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>List all Folders in User's Google Drive that matches the Filter criteria.</summary>
		/// <param name="pFilter">Custom Filter, check https://developers.google.com/drive/api/guides/search-files for details.</param>
		/// <returns>a List with all found Folders</returns>
		public async Task<List<File>> FindFolders(string pFilter)
		{
			List<File> _ret = null;
			try
			{
				await Task.Run(() =>
				{
					if (DService != null)
					{
						//Google Drive API v3 Search query terms and operators -> https://developers.google.com/drive/api/guides/ref-search-terms
						// https://developers.google.com/drive/api/reference/rest/v3/files

						FilesResource.ListRequest FilesRequest = DService.Files.List();
						FilesRequest.Spaces = "drive";
						FilesRequest.Fields = "nextPageToken, files(id, name, mimeType, createdTime, modifiedTime, description, size, parents)";
						FilesRequest.PageSize = 100;    //Pagination: If there are more files than the specified page size,
														//use the nextPageToken property in the response to fetch subsequent pages			

						if (!string.IsNullOrEmpty(pFilter))
						{
							pFilter = pFilter.Replace("*", string.Empty);
							FilesRequest.Q = string.Format(this.FolderFilter, pFilter);
						}

						if (FilesRequest != null)
						{
							// Retrieve file metadata
							this.Files = FilesRequest.Execute().Files.ToList();
							int Counter = 1;
							foreach (var file in Files)
							{
								if (file.Parents != null)
								{
									foreach (var parent in file.Parents)
									{
										var pData = DService.Files.Get(parent);
										if (pData != null)
										{
											var xx = pData.Execute();
											file.Name = System.IO.Path.Combine(xx.Name, file.Name);
										}
									}
								}
								Console.WriteLine($"{Counter} - File Name: {file.Name}, File Type: {file.MimeType}, File ID: {file.Id}");
								Counter++;
							}
							_ret = Files;
						}
					}
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion

		#region Create / Modify

		/// <summary>Creates a new Folder in the User's Google Drive.</summary>
		/// <param name="pFolderName">Name for the new Folder</param>
		/// <param name="pDescription">Description of the new Folder</param>
		/// <returns>the newly created Folder</returns>
		public async Task<File> CreateFolder(string pFolderName, string pDescription = "")
		{
			File _ret = null;
			await Task.Run(() =>
			{
				// Create file metadata for the new folder
				_ret = new File()
				{
					Name = pFolderName,
					Description = pDescription,
					MimeType = "application/vnd.google-apps.folder"
				};

				// Create the folder
				try
				{
					_ret = DService.Files.Create(_ret).Execute();
					_ret.ModifiedTime = DateTime.Now;
					_ret.ModifiedTimeRaw = DateTime.Now.ToString();
					Console.WriteLine("Folder created: {0}", _ret.Id);
				}
				catch (Exception ex)
				{
					throw ex;
				}
			});
			return _ret;
		}

		/// <summary>Creates and Uploads a new File to the User's Google Drive.</summary>
		/// <param name="pFilePath">Full Path to the local file that will be Uploaded.</param>
		/// <param name="FolderID">[OPTIONAL] ID of the Parent folder in GDrive. empty = Root</param>
		/// <param name="pDescription">[OPTIONAL] Description of the new Folder</param>
		/// <returns>the newly created File</returns>
		public async Task<File> CreateFile(string pFilePath, string FolderID = "", string pDescription = "")
		{
			File _ret = null;
			await Task.Run(() =>
			{
				try
				{
					if (System.IO.File.Exists(pFilePath))
					{
						// Upload file photo.jpg on drive.
						var fileMetadata = new Google.Apis.Drive.v3.Data.File()
						{
							Name = System.IO.Path.GetFileName(pFilePath),  // Extract filename from path
							MimeType = GetCorrectMimeType(pFilePath),  // Determine correct MIME type
							Description = pDescription
						};
						if (!string.IsNullOrEmpty(FolderID))
						{
							fileMetadata.Parents = new List<string>() { FolderID };  // Set parent folder ID
						}
						FilesResource.CreateMediaUpload request;

						// Create a new file on drive.
						using (var stream = new System.IO.FileStream(pFilePath, System.IO.FileMode.Open))
						{
							// Create a new file, with metadata and stream.
							request = DService.Files.Create(fileMetadata, stream, fileMetadata.MimeType);
							request.Fields = "id";
							var uProgress = request.Upload();
							if (uProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
							{
								throw new Exception(uProgress.Exception.Message);
							}
						}
						_ret = request.ResponseBody;
						_ret.ModifiedTime = DateTime.Now;
						_ret.ModifiedTimeRaw = DateTime.Now.ToString();
					}
					else
					{
						throw new System.IO.FileNotFoundException("Eror 404 - Not Found.\r\nThe File doesn't exists in the local 'pFilePath'.");
					}
				}
				catch (Exception e)
				{
					if (e is AggregateException)
					{
						Console.WriteLine("Credential Not found");
					}
					else if (e is System.IO.FileNotFoundException)
					{
						Console.WriteLine("File not found");
					}
					throw;
				}
			});
			return _ret;
		}

		/// <summary>Upload and Update changes in an existing GDrive File.</summary>
		/// <param name="pFilePath">Full Path to the local file that will be Uploaded.</param>
		/// <param name="FileID">ID of an existing File on Google Drive</param>
		/// <returns>the Updated File</returns>
		public async Task<File> UpdateFile(string pFilePath, string FileID)
		{
			File _ret = null;
			await Task.Run(() =>
			{
				try
				{
					if (System.IO.File.Exists(pFilePath))
					{
						var fileMetadata = new Google.Apis.Drive.v3.Data.File()
						{
							Name = System.IO.Path.GetFileName(pFilePath),  // Extract filename from path
							MimeType = GetCorrectMimeType(pFilePath),
						};

						if (fileMetadata != null)
						{
							FilesResource.UpdateMediaUpload updateRequest;
							using (var upload_stream = new System.IO.FileStream(pFilePath, System.IO.FileMode.Open))
							{
								long totalFileSize = upload_stream.Length;  // Get total file size
								updateRequest = DService.Files.Update(fileMetadata, FileID, upload_stream, fileMetadata.MimeType);
								updateRequest.Fields = "name,mimeType";
								//updateRequest.Upload();
								Google.Apis.Upload.IUploadProgress uProgress = updateRequest.Upload();
								//while (uProgress.Status != Google.Apis.Upload.UploadStatus.Completed)
								//{
								//	if (uProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
								//	{
								//		throw new Exception(uProgress.Exception.Message);
								//	}
								//	// Update progress bar or display status message
								//	decimal PercentageDone = (decimal)(uProgress.BytesSent * 100 / totalFileSize);
								//	OnProgressChange?.Invoke(PercentageDone, EventArgs.Empty);
								//	Thread.Sleep(500);  // Adjust polling interval as needed
								//}

								if (uProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
								{
									throw new Exception(uProgress.Exception.Message);
								}
								// Get the uploaded file metadata
								_ret = updateRequest.ResponseBody;
								_ret.Id = FileID;
								_ret.ModifiedTime = DateTime.Now;
								_ret.ModifiedTimeRaw = DateTime.Now.ToString();
								this.FileData = _ret;
							}
						}
						else
						{
							throw new Exception("Eror 404 - Not Found.\r\nThe File doesn't exists on GoogeDrive.");
						}
					}
					else
					{
						throw new Exception("Eror 404 - Not Found.\r\nThe File doesn't exists in the local 'pFilePath'.");
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}
			});
			return _ret;
		}

		public string GetCorrectMimeType(string filePath)
		{
			// Use a built-in MIME type mapping for common file extensions
			var mimeTypes = new Dictionary<string, string>
			{
				{ ".txt", "text/plain" },
				{ ".doc", "application/msword" },
				{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
				{ ".xls", "application/vnd.ms-excel" },
				{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
				{ ".ppt", "application/vnd.ms-powerpoint" },
				{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
				{ ".pdf", "application/pdf" },
				{ ".jpg", "image/jpeg" },
				{ ".png", "image/png" },
				{ ".gif", "image/gif" },
				// Add more mappings as needed
			};

			string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
			if (mimeTypes.ContainsKey(extension))
			{
				return mimeTypes[extension];
			}
			else
			{
				// Use a more sophisticated method for unknown extensions,
				// such as using the System.Web.MimeMapping.GetMimeMapping method
				// or a third-party library for MIME type detection
				string mimeType = MimeMapping.GetMimeMapping(filePath);
				return mimeType ?? "application/octet-stream";  // Default to generic binary type
			}
		}

		#endregion

		#region Download

		/// <summary>Download and Save the selected file. Will prompt for save location.
		/// <para>Use first the 'GetFile' method to select the desired file.</para>
		/// </summary>
		/// <param name="pFilter">Filter for the 'Save Dialog'</param>
		/// <param name="pDefaultExt">Default File Extension for the 'Save Dialog'</param>
		/// <returns>The full path to the downloaded file</returns>
		public async Task<string> DownloadFile(string FileID, string pFilter = "MyNotes document|*.note|All Files|*.*", string pDefaultExt = "note")
		{
			string _ret = string.Empty;
			try
			{
				SaveFileDialog SFDialog = new SaveFileDialog()
				{
					Filter = pFilter,
					FilterIndex = 0,
					DefaultExt = pDefaultExt,
					AddExtension = true,
					CheckPathExists = true,
					OverwritePrompt = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};

				if (SFDialog.ShowDialog() == DialogResult.OK)
				{
					await Task.Run(() =>
					{
						DownloadedFilePath = SFDialog.FileName;

						FilesResource.GetRequest request = DService.Files.Get(FileID);
						request.Fields = "id, name, mimeType, createdTime, modifiedTime, size, parents";
						this.FileData = request.Execute();

						using (var stream = new System.IO.FileStream(DownloadedFilePath, System.IO.FileMode.Create))
						{
							long totalFileSize = (long)FileData.Size;  // Get total file size

							var uProgress = request.DownloadWithStatus(stream);
							//while (uProgress.Status != Google.Apis.Download.DownloadStatus.Completed)
							//{
							//	// Update progress bar or display status message
							//	decimal PercentageDone = (decimal)(uProgress.BytesDownloaded * 100 / totalFileSize);
							//	OnProgressChange?.Invoke(PercentageDone, EventArgs.Empty);
							//	//Thread.Sleep(500);  // Adjust polling interval as needed
							//}
							_ret = DownloadedFilePath;
							//
						}
					});
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		#endregion
	}
}
