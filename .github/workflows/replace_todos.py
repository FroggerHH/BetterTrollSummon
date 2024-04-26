import os
import re


class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'


def find_new_todos(file_extensions):
    new_todos = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(tuple(file_extensions)):
                file_path = os.path.join(root, file)
                with open(file_path, "r", encoding="utf-8") as f:
                    content = f.read()

                todos_found = re.findall(new_todo_pattern, content)
                if todos_found:
                    print(f'Found {len(todos_found)} todos in file {file_path.split('/')[-1]}')
                    for todo in todos_found:
                        print(' -', todo)
                    new_todos.extend(todos_found)

    return new_todos


def overwrite_todos(new_todos):
    with open(readme_path, 'r') as file:
        content = file.read()
    content = re.sub(old_todo_pattern, '## TODOs\n', content)
    if '## TODOs' not in content:
        print(bcolors.WARNING + 'TODOs section not found in README.md. Action will add it in the end of file.')
        print(bcolors.WARNING + 'You need to add a TODOs section to README.md. Can be empty, just a header.')
        print(bcolors.WARNING + 'Example: ## TODOs' + bcolors.ENDC)

        with open(readme_path, "w", encoding="utf-8") as f:
            f.write(content)
            f.write('\n## TODOs\n')
        with open(readme_path, 'r') as file:
            content = file.read()

    end_of_todos_pattern = re.compile(r'## TODOs\n')
    end_of_todos_match = end_of_todos_pattern.search(content)

    if end_of_todos_match:
        match_end = end_of_todos_match.end()
        middle = ''
        for i in range(len(new_todos)):
            todo = new_todos[i]
            # middle += f'{i+1}. {todo}\n'
            middle += f'* {todo}\n'
        content = content[:match_end] + middle + content[match_end:]

    with open(readme_path, "w", encoding="utf-8") as f:
        f.write(content)


if __name__ == "__main__":
    directory = os.getcwd()
    print('Looking for new todos in directory', directory)
    new_todo_pattern = re.compile(r"//TODO:\s*([^\n]+)")
    old_todo_pattern = re.compile(r'#+\s*TODOs(.*?)(?=\n?#+|$)', re.DOTALL)
    readme_path = os.path.join(directory, 'README.md')
    overwrite_todos(find_new_todos(['.cs']))