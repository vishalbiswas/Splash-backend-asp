CREATE TABLE [dbo].[attachments] (
    [attachid] BIGINT          IDENTITY (1, 1) NOT NULL,
    [image]    VARBINARY (MAX) DEFAULT (NULL) NULL,
    [size]     BIGINT          DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([attachid] ASC)
);

