export interface MenuItem {
  path: string;
  label: string;
  icon: string;
  roles: string[]; // Ex: ['Master'] ou ['Admin'] ou ['Master', 'Admin']
}