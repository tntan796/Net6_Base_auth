CREATE OR ALTER    PROCEDURE [dbo].[GenEntity]
  @TableName NVARCHAR(200)
AS
BEGIN
   SET NOCOUNT ON
   
declare @Result varchar(max) = 'public class ' + @TableName + '
{'

select @Result = @Result + '
    public ' + ColumnType + NullableSign + ' ' + ColumnName + ' { get; set; }
'
from
(
    select 
        replace(col.name, ' ', '_') ColumnName,
        column_id ColumnId,
        case typ.name 
            when 'bigint' then 'long'
            when 'binary' then 'byte[]'
            when 'bit' then 'bool'
            when 'char' then 'string'
            when 'date' then 'DateTime'
            when 'datetime' then 'DateTime'
            when 'datetime2' then 'DateTime'
            when 'datetimeoffset' then 'DateTimeOffset'
            when 'decimal' then 'decimal'
            when 'float' then 'double'
            when 'image' then 'byte[]'
            when 'int' then 'int'
            when 'money' then 'decimal'
            when 'nchar' then 'string'
            when 'ntext' then 'string'
            when 'numeric' then 'decimal'
            when 'nvarchar' then 'string'
            when 'real' then 'float'
            when 'smalldatetime' then 'DateTime'
            when 'smallint' then 'short'
            when 'smallmoney' then 'decimal'
            when 'text' then 'string'
            when 'time' then 'TimeSpan'
            when 'timestamp' then 'long'
            when 'tinyint' then 'byte'
            when 'uniqueidentifier' then 'Guid'
            when 'varbinary' then 'byte[]'
            when 'varchar' then 'string'
            else 'UNKNOWN_' + typ.name
        end ColumnType,
        case 
            when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier') 
            then '?' 
            else '' 
        end NullableSign
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @Result = @Result  + '
}'

print @Result
END
GO
CREATE OR ALTER   PROCEDURE [dbo].[GenStore]
  @TableName NVARCHAR(200),
  @Name NVARCHAR(200)
AS
BEGIN
   SET NOCOUNT ON
   
   SELECT 
        REPLACE(col.name, ' ', '_') ColumnName,
        column_id ColumnId,
		typ.name AS ColumnType,
		CASE WHEN typ.name = 'varchar' THEN '(' + CONVERT(VARCHAR(50),col.max_length) + ')'
		     WHEN typ.name NOT LIKE '%varchar%' THEN ''
		     WHEN typ.name = 'nvarchar' AND col.max_length > 0 THEN '(' + CONVERT(VARCHAR(10),col.max_length/2) + ')'
		     WHEN col.max_length < 0 THEN '(MAX)'
			 ELSE ''
		END AS MaxLengthValue
    INTO #TempColumns
    FROM sys.columns col join sys.types typ on  col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    WHERE object_id = object_id(@TableName)

--Store CREATE
  DECLARE @Store_Create nvarchar(max) 
  SET @Store_Create = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_Create]'

  DECLARE @Store_Column_Create NVARCHAR(MAX)
  SELECT  @Store_Column_Create = COALESCE(@Store_Column_Create + ',','') + '
  @' + ColumnName + ' ' + UPPER(ColumnType)  + MaxLengthValue 
  FROM #TempColumns
  WHERE ColumnName NOT IN ('Id', 'CreatedAt', 'UpdatedAt', 'UpdatedBy', 'IsDeleted')
  ORDER BY ColumnId
  SET @Store_Column_Create = STUFF(@Store_Column_Create, 1, 1, '')
  SET @Store_Create = @Store_Create + @Store_Column_Create
  SET @Store_Create = @Store_Create + '
AS' + '
BEGIN
    INSERT INTO ' + @TableName + '('

  SET @Store_Column_Create = ''
  SELECT  @Store_Column_Create = COALESCE(@Store_Column_Create + ',','') + ColumnName 
  FROM #TempColumns
  ORDER BY ColumnId
  SET @Store_Column_Create = STUFF(@Store_Column_Create, 1, 1, '')
  SET @Store_Create = @Store_Create + @Store_Column_Create

  SET @Store_Create = @Store_Create + ')
    VALUES('
  SET @Store_Column_Create = ''
  SELECT  @Store_Column_Create = COALESCE(@Store_Column_Create + ',','') + 
          CASE WHEN ColumnName = 'Id' THEN 'NEWID()'
		       WHEN ColumnName = 'IsDeleted' THEN '0'
		       WHEN ColumnName IN ('CreatedAt', 'UpdatedAt') THEN 'GETDATE()'
			   WHEN ColumnName = 'UpdatedBy' THEN '@CreatedBy'
		       ELSE '@' + ColumnName 
		  END
  FROM #TempColumns
  ORDER BY ColumnId
  SET @Store_Column_Create = STUFF(@Store_Column_Create, 1, 1, '')
  SET @Store_Create = @Store_Create + @Store_Column_Create + ')
END'

  PRINT @Store_Create
  PRINT '
  GO
  '
--Store UPDATE
  DECLARE @Store_Update nvarchar(max) 
  SET @Store_Update = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_Update]'

  DECLARE @Store_Column_Update NVARCHAR(MAX)
  SELECT  @Store_Column_Update = COALESCE(@Store_Column_Update + ',','') + '
  @' + ColumnName + ' ' + UPPER(ColumnType)  + MaxLengthValue 
  FROM #TempColumns
  WHERE ColumnName NOT IN ('CreatedBy', 'CreatedAt', 'UpdatedAt', 'IsDeleted')
  ORDER BY ColumnId
  SET @Store_Column_Update = STUFF(@Store_Column_Update, 1, 1, '')
  SET @Store_Update = @Store_Update + @Store_Column_Update
  SET @Store_Update = @Store_Update + '
AS' + '
BEGIN
    BEGIN TRANSACTION
	BEGIN TRY
	'
   IF EXISTS (SELECT ColumnName FROM #TempColumns WHERE ColumnName LIKE '%File%')
   BEGIN
     SELECT ColumnName INTO #ColumnFile FROM #TempColumns WHERE ColumnName LIKE '%File%'
	 DECLARE @ColumnFile NVARCHAR(20)
	 WHILE EXISTS (SELECT ColumnName FROM #ColumnFile)
	 BEGIN
	    SET @ColumnFile = (SELECT TOP(1) ColumnName FROM #ColumnFile)
	    SET @Store_Update = @Store_Update + '
		IF NOT EXISTS (SELECT Id FROM dbo.' + @TableName + ' WHERE Id = @id AND '+ @ColumnFile + '=@' + @ColumnFile +')
		   UPDATE dbo.Files SET IsDeleted = 1 WHERE Id = (SELECT '+@ColumnFile+' FROM dbo.'+@TableName+' WHERE Id = @Id)
		'
	    DELETE FROM #ColumnFile WHERE ColumnName = @ColumnFile
	 END
   END

  SET @Store_Update = @Store_Update + '
    UPDATE ' + @TableName + '
	SET 
	'
  SET @Store_Column_Update = ''
  SELECT  @Store_Column_Update = COALESCE(@Store_Column_Update + ',','') + ColumnName + '=' + 
      CASE WHEN ColumnName = 'UpdatedAt' THEN 'GETDATE()
	   '
								 ELSE + '@' + ColumnName + '
	   ' END
  FROM #TempColumns
  WHERE ColumnName NOT IN ('Id','CreatedBy', 'CreatedAt', 'IsDeleted')
  ORDER BY ColumnId
  SET @Store_Column_Update = STUFF(@Store_Column_Update, 1, 1, '')
  SET @Store_Update = @Store_Update + @Store_Column_Update + '
     WHERE Id=@Id

	    COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		   ROLLBACK TRANSACTION
		DECLARE @ErrorMessage nvarchar(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()
		RAISERROR (@ErrorMessage, 16, 1);
	END CATCH
END'

  PRINT @Store_Update
  PRINT '
  GO
  '
 
 --Store DELETE ONE
  DECLARE @Store_Delete nvarchar(max) 
  SET @Store_Delete = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_Delete]
   @Id UNIQUEIDENTIFIER
  AS
  BEGIN
    UPDATE ' + @TableName + ' SET IsDeleted = 1 WHERE Id = @Id
  END'

  PRINT @Store_Delete
  PRINT '
  GO
  '
--Store DELETE MANY
  DECLARE @Store_DeleteMany nvarchar(max) 
  SET @Store_DeleteMany = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_DeleteMany]
   @ListId NVARCHAR(MAX)
  AS
  BEGIN
    UPDATE A SET A.IsDeleted = 1 FROM ' + @TableName + ' A INNER JOIN STRING_SPLIT(@ListId,'';'') B ON A.Id = B.value
  END'

  PRINT @Store_DeleteMany
  PRINT '
  GO
  '

--Store GetDetail
  DECLARE @Store_Detail nvarchar(max) 
  SET @Store_Detail = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_GetDetail]
   @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT '

  DECLARE @Store_Column_Detail NVARCHAR(MAX)
  SELECT  @Store_Column_Detail = COALESCE(@Store_Column_Detail + ',','') + 'A.' + ColumnName + '
          '
  FROM #TempColumns
  WHERE ColumnName NOT IN ('CreatedBy', 'CreatedAt', 'UpdatedBy','UpdatedAt', 'IsDeleted')
  ORDER BY ColumnId
  SET @Store_Column_Detail = STUFF(@Store_Column_Detail, 1, 0, '')
  SET @Store_Detail = @Store_Detail + @Store_Column_Detail + '
    FROM ' + @TableName + ' A
	WHERE ISNULL(A.IsDeleted,0) = 0 AND A.Id=@Id' + '
END'

  PRINT @Store_Detail
  PRINT '
  GO
  '

--Store GetList
  DECLARE @Store_GetList nvarchar(max) 
  SET @Store_GetList = '--'+@Name+': ' + CONVERT(NVARCHAR(30),GETDATE(),105) + '
CREATE OR ALTER PROCEDURE [dbo].[Proc_' + @TableName + '_GetList]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUT
AS
BEGIN
    SELECT ROW_NUMBER() OVER(ORDER BY A.CreatedAt DESC) AS Stt,
	'

  DECLARE @Store_Column_GetList NVARCHAR(MAX)
  SELECT  @Store_Column_GetList = COALESCE(@Store_Column_GetList + ',','') + 'A.' + ColumnName + '
          '
  FROM #TempColumns
  WHERE ColumnName NOT IN ('CreatedBy', 'CreatedAt', 'UpdatedBy','UpdatedAt', 'IsDeleted')
  ORDER BY ColumnId
  SET @Store_Column_GetList = STUFF(@Store_Column_GetList, 1, 0, '')
  SET @Store_GetList = @Store_GetList + @Store_Column_GetList + '
    INTO #TempResult
    FROM ' + @TableName + ' A
	WHERE ISNULL(A.IsDeleted,0) = 0' + '
	SELECT * FROM #TempResult ORDER BY Stt OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
    SELECT @TotalCount = COUNT(Id) FROM #TempResult

END'

  PRINT @Store_GetList
END
GO
CREATE OR ALTER PROCEDURE [dbo].[Proc_GetListParam]
 @StoreName NVARCHAR(200)
AS
BEGIN
  SELECT REPLACE(name,'@','') as param  from sys.parameters where object_id = object_id(@StoreName)
END
GO


