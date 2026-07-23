/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * Button 参数
 */
export interface IButtonParams {
  name?: string,
  label?: string,
  tooltip?: string,
  icon?: string,
  dense?: boolean,
  color?: string,
  loading?: boolean,
  size: string,
  onClick: (evt?: Event, go?: Promise<any>, btnParams?: IButtonParams) => void,
}
