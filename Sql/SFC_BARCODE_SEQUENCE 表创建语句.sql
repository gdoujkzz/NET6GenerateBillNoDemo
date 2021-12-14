-- Create table
create table SFC_BARCODE_SEQUENCE
(
  id                VARCHAR2(36 CHAR) default sys_guid() not null,
  datetime_created  DATE default sysdate not null,
  user_created      VARCHAR2(80 CHAR) default 'SYS' not null,
  datetime_modified DATE,
  user_modified     VARCHAR2(80 CHAR),
  state             CHAR(1) default 'A' not null,
  enterprise_id     VARCHAR2(36 CHAR) default '*' not null,
  org_id            VARCHAR2(36 CHAR) not null,
  barcode_category  VARCHAR2(80 CHAR) not null,
  prefix            VARCHAR2(80 CHAR) not null,
  current_value     NUMBER(22) not null,
  barcode_rule      VARCHAR2(80 CHAR) not null
)
tablespace WMSD
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
-- Add comments to the columns 
comment on column SFC_BARCODE_SEQUENCE.id
  is '数据行 ID 号';
comment on column SFC_BARCODE_SEQUENCE.datetime_created
  is '创建数据行的时间';
comment on column SFC_BARCODE_SEQUENCE.user_created
  is '创建人';
comment on column SFC_BARCODE_SEQUENCE.datetime_modified
  is '更新数据行的时间';
comment on column SFC_BARCODE_SEQUENCE.user_modified
  is '更新数据行的用户';
comment on column SFC_BARCODE_SEQUENCE.state
  is '可用状态';
comment on column SFC_BARCODE_SEQUENCE.org_id
  is '所属组织 ID';
comment on column SFC_BARCODE_SEQUENCE.barcode_category
  is '条码类别';
comment on column SFC_BARCODE_SEQUENCE.prefix
  is '条码前缀';
comment on column SFC_BARCODE_SEQUENCE.current_value
  is '当前最大流水号';
comment on column SFC_BARCODE_SEQUENCE.barcode_rule
  is '生成时使用的条码规则';
-- Create/Recreate indexes 
create unique index IX_SFC_BARCODE_SEQUENCE on SFC_BARCODE_SEQUENCE (ENTERPRISE_ID, ORG_ID, BARCODE_CATEGORY, PREFIX)
  tablespace MESX
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
-- Create/Recreate primary, unique and foreign key constraints 
alter table SFC_BARCODE_SEQUENCE
  add constraint PK_SFC_BARCODE_SEQUENCE primary key (ID)
  using index 
  tablespace MESD
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
