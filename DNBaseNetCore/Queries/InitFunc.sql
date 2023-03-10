--DVNHAT: 22-03-2022: Lấy đường dẫn file
CREATE OR ALTER  FUNCTION  [dbo].[Func_GetPathFile]
(
	@EntityId UNIQUEIDENTIFIER,
	@TypeFile VARCHAR(100)
)
RETURNS NVARCHAR(1000)
AS
BEGIN
	RETURN (SELECT TOP(1) Path FROM dbo.Files WHERE EntityId = @EntityId AND FileTypeUpload=@TypeFile  AND IsDeleted = 0)
END
GO
--DVNHAT: 22-03-2022 : Lấy thông tin của file
CREATE OR ALTER  FUNCTION [dbo].[Func_GetInfoFile]
(
    @EntityName VARCHAR(100),
	@TypeFile VARCHAR(100)
)
RETURNS 
@ReturnTable TABLE 
(
     Id UNIQUEIDENTIFIER,
     EntityId UNIQUEIDENTIFIER,
	 Path NVARCHAR(1000),
     Name NVARCHAR(200),
     Size NVARCHAR(100)
)
AS
BEGIN
            INSERT INTO @ReturnTable
			SELECT 
			       Id,
				   EntityId, 
				   Path ,
				   Name,
				   CONVERT(VARCHAR(10), CONVERT(FLOAT(2), ROUND(Size/(1024*1024),2))) + 'MB' AS Size
			FROM dbo.Files 
			WHERE EntityName = @EntityName AND FileTypeUpload=@TypeFile	AND IsDeleted = 0
    RETURN 
END
GO
--DVNHAT: 22-03-2022: Lấy danh mục
CREATE OR ALTER FUNCTION [dbo].[Func_GetCategoryItem] 
(
    @Code varchar(50)
)
RETURNS 
@ReturnTable TABLE 
(
     Id uniqueidentifier,
	 Description nvarchar(500),
     Code nvarchar(50),
     Name nvarchar(200)
)
AS
BEGIN
            INSERT INTO @ReturnTable
            SELECT A.Id, A.Description, A.Code, A.Name FROM dbo.CategoryItems A INNER JOIN dbo.Categories B ON B.Id = A.CategoryId AND B.Code = @Code
    RETURN 
END

