namespace AcadLib.Tables.Google
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using Autodesk.AutoCAD.DatabaseServices;
	using DynamicData;
	using global::Google.Apis.Sheets.v4.Data;
	using GoogleDoc;
	using NetLib;

	public class GoogleTable
	{
		private Table  _table;
		private int    _sheetId;
		private double _scale;

		public async Task CreateTable(Table table, string fileName, string sheetName, double scale)
		{
			try
			{
				_scale = scale;
				_table = table;
				using (var drive = new DriveUtils())
				{
					var fileId = await drive.CreateSpreadsheetFile(fileName);
					var ws     = Worksheet.CreateEndUser(fileId);
					_sheetId = ws.CreateSheet(sheetName);
					var response = await ws.BatchUpdate(GetBatchUpdate(), fileId);
					Utils.ShowSpreadsheetFile(fileId, _sheetId.ToString());
				}
			}
			catch (Exception ex)
			{
				Logger.Log.Error(ex, "GoogleTable");
				MessageBox.Show(ex.Message);
			}
		}

		private BatchUpdateSpreadsheetRequest GetBatchUpdate()
		{
			var batch = new BatchUpdateSpreadsheetRequest
			{
				Requests = new List<Request>
				{
					new Request { DeleteSheet = new DeleteSheetRequest { SheetId = 0 } },
					new Request
					{
						UpdateCells = new UpdateCellsRequest
						{
							Fields = "*",
							Start  = new GridCoordinate { SheetId = _sheetId },
							Rows   = GetRows()
						},
					}
				}
			};

			batch.Requests.AddRange(GetMerges());
			batch.Requests.AddRange(GetColumnWidths());
			return batch;
		}

		private IList<RowData> GetRows()
		{
			var rows = _table.Rows.Select(s =>
				new RowData
				{
					Values = s.Select(c => GetCell(_table.Cells[c.Row, c.Column])).ToList()
				}).ToList();
			rows.Insert(0, new RowData
			{
				Values = new List<CellData>
				{
					new CellData
					{
						UserEnteredValue = new ExtendedValue
						{
							StringValue = "Выгружаемые данные только для служебного пользования в ПИК-Проект"
						}
					}
				}
			});
			return rows;
		}

		private CellData GetCell(Cell cel)
		{
			return new CellData
			{
				UserEnteredValue  = GetValue(cel),
				UserEnteredFormat = GetCellFormat(cel),
			};
		}

		private static ExtendedValue GetValue(Cell cel)
		{
			if (cel.Value == null || cel.Value.ToString().IsNullOrEmpty()) return null;
			switch (cel.Value)
			{
				case double d: return new ExtendedValue { NumberValue = d };
				case int i:    return new ExtendedValue { NumberValue = i };
				case bool b:   return new ExtendedValue { BoolValue   = b };
			}

			return new ExtendedValue { StringValue = cel.Value?.ToString() };
		}

		private IEnumerable<Request> GetColumnWidths()
		{
			return _table.Columns.Select((c, i) => new Request
			{
				UpdateDimensionProperties = new UpdateDimensionPropertiesRequest
				{
					Range = new DimensionRange
					{
						SheetId    = _sheetId,
						Dimension  = "COLUMNS",
						StartIndex = i,
						EndIndex   = i + 1,
					},
					Properties = new DimensionProperties
					{
						PixelSize = (int)(c.Width * 5 / _scale),
					},
					Fields = "pixelSize"
				}
			});
		}

		private IEnumerable<Request> GetMerges()
		{
			foreach (var r in _table.Cells)
			{
				var c = _table.Cells[r.Row, r.Column];
				if (c.IsMerged != true) continue;
				Debug.WriteLine($"{c.Row}-{c.Column} = {c.Value}");
				var m = c.GetMergeRange();
				yield return new Request
				{
					MergeCells = new MergeCellsRequest
					{
						Range = new GridRange
						{
							SheetId          = _sheetId,
							StartRowIndex    = m.TopRow    + 1,
							EndRowIndex      = m.BottomRow + 2,
							StartColumnIndex = m.LeftColumn,
							EndColumnIndex   = m.RightColumn + 1,
						}
					}
				};
			}
		}

		private CellFormat GetCellFormat(Cell cel)
		{
			var format = new CellFormat { BackgroundColor = GetColor(cel.BackgroundColor) };
			SetAlignment(cel, format);
			if (cel.Row == 1)
			{
				format.WrapStrategy = "WRAP";
			}

			return format;
		}

		private void SetAlignment(Cell cel, CellFormat format)
		{
			switch (cel.Alignment)
			{
				case CellAlignment.TopLeft:
					format.HorizontalAlignment = "LEFT";
					format.VerticalAlignment   = "TOP";
					break;
				case CellAlignment.TopCenter:
					format.HorizontalAlignment = "CENTER";
					format.VerticalAlignment   = "TOP";
					break;
				case CellAlignment.TopRight:
					format.HorizontalAlignment = "RIGHT";
					format.VerticalAlignment   = "TOP";
					break;
				case CellAlignment.MiddleLeft:
					format.HorizontalAlignment = "LEFT";
					format.VerticalAlignment   = "MIDDLE";
					break;
				case CellAlignment.MiddleCenter:
					format.HorizontalAlignment = "CENTER";
					format.VerticalAlignment   = "MIDDLE";
					break;
				case CellAlignment.MiddleRight:
					format.HorizontalAlignment = "RIGHT";
					format.VerticalAlignment   = "MIDDLE";
					break;
				case CellAlignment.BottomLeft:
					format.HorizontalAlignment = "LEFT";
					format.VerticalAlignment   = "BOTTOM";
					break;
				case CellAlignment.BottomCenter:
					format.HorizontalAlignment = "CENTER";
					format.VerticalAlignment   = "BOTTOM";
					break;
				case CellAlignment.BottomRight:
					format.HorizontalAlignment = "RIGHT";
					format.VerticalAlignment   = "BOTTOM";
					break;
				case null:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private Color GetColor(global::Autodesk.AutoCAD.Colors.Color color)
		{
			if (color == null || color.IsNone) return null;
			return new Color
			{
				Red   = color.Red   / 255f,
				Blue  = color.Blue  / 255f,
				Green = color.Green / 255f,
			};
		}
	}
}
