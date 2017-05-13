CREATE TABLE [dbo].[comments] (
    [commentid]  BIGINT   IDENTITY (1, 1) NOT NULL,
    [threadid]   BIGINT   NULL,
    [content]    TEXT     NOT NULL,
    [creator_id] BIGINT   NULL,
    [ctime]      DATETIME DEFAULT (getdate()) NOT NULL,
    [mtime]      DATETIME DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([commentid] ASC),
    FOREIGN KEY ([creator_id]) REFERENCES [dbo].[users] ([uid]),
    FOREIGN KEY ([threadid]) REFERENCES [dbo].[threads] ([threadid])
);

